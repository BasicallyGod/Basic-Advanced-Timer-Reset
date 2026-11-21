'use strict';
const $ = id => document.getElementById(id);
const pending = new Map();
let state, current = null, dirty = false, saving = null, editSequence = 0, saveTimer;
let view = 'timesheets', range = 'full', allReminders = false, linkReturn;
let captureBusy = false;
let saveError = '';
function banner(message) { $('banner').textContent = message; $('banner').hidden = !message; }
function post(type, payload = {}) {
  const requestId = crypto.randomUUID();
  const request = {version:1, requestId, type, payload};
  if (!window.chrome?.webview) return Promise.reject(new Error('Open this frame in the BAT Windows host.'));
  return new Promise((resolve, reject) => { pending.set(requestId, {resolve, reject}); window.chrome.webview.postMessage(request); });
}
function receive(event) {
  const {type, payload} = event.data;
  if (type === 'reply') {
    state = payload.state; renderState();
    const item = pending.get(payload.requestId); pending.delete(payload.requestId);
    if (payload.ok) item?.resolve(payload); else item?.reject(new Error(payload.error));
  }
  if (type === 'state') { state = payload; banner(state.notice); renderState(); }
  if (type === 'clock') { state = payload; renderClock(); renderReminderQueue(); }
  if (type === 'capture') capture(payload);
  if (type === 'prepareQuit') flush().then(ok=>notify('host.quitReady',{ok}));
  if (type === 'storageError') banner(payload);
}
window.chrome?.webview?.addEventListener('message', receive);
function notify(type,payload={}) { window.chrome?.webview?.postMessage({version:1,requestId:crypto.randomUUID(),type,payload}); }
function setView(next) {
  view = next;
  $('dashboard').hidden = next !== 'timesheets'; $('notes-workspace').hidden = next !== 'notes';
  for (const name of ['timesheets','notes']) { $(name).removeAttribute('aria-current'); if (name === next) $(name).setAttribute('aria-current','page'); }
  $('workspace-title').textContent = next === 'notes' ? 'A place to pick up the thread.' : 'A little space for your day.';
  sessionStorage.setItem('bat-view', next);
}
function text(tag, content, className) { const node = document.createElement(tag); node.textContent = content; if (className) node.className = className; return node; }
function caption(note) { return note.title.trim() || note.body.split('\n').find(s => s.trim())?.replace(/^#+\s*/, '').replace(/\[([^\]]+)\]\([^)]+\)/g,'$1').slice(0,100) || 'Untitled note'; }
function renderState() { renderClock(); renderNotes(); renderReminderQueue(); renderTimeline(); renderRecent(); }
function renderClock() {
  if (!state) return;
  const seconds = Math.max(0, Math.floor(state.elapsedSeconds));
  $('clock').textContent = [Math.floor(seconds/3600), Math.floor(seconds/60)%60, seconds%60].map(n=>String(n).padStart(2,'0')).join(':');
  $('timer-toggle').textContent = state.activeAccount ? 'Pause recording' : 'Start recording';
  $('timer-account').disabled = !!state.activeAccount;
  if (state.activeAccount) $('timer-account').value = state.activeAccount;
  $('record-dot').classList.toggle('active', !!state.activeAccount);
  $('timer-status').textContent = state.activeAccount ? 'Recording for '+state.activeAccount : 'Ready when you need it';
  $('date').textContent = new Date(state.now).toLocaleDateString(undefined,{weekday:'long',day:'numeric',month:'long'});
}
function renderNotes() {
  if (!state) return;
  const query = $('search').value.toLowerCase(), account = $('account-filter').value;
  const notes = state.workspace.notes.filter(n => (!account || (account === 'unassigned' ? !n.account : n.account === account)) && (n.title+'\n'+n.body).toLowerCase().includes(query)).sort((a,b)=>b.updated.localeCompare(a.updated));
  const list = $('note-list'); list.replaceChildren();
  for (const note of notes) {
    const row = text('button','', 'note-row'); row.dataset.id = note.id;
    if (current?.id === note.id) row.setAttribute('aria-current','true');
    row.append(text('strong',caption(note)));
    const at = query ? note.body.toLowerCase().indexOf(query) : 0;
    const snippet = note.body.slice(Math.max(0,at-35),Math.max(0,at-35)+105).replace(/\n/g,' ');
    row.append(text('small',(note.account || 'Unassigned') + (snippet ? ' · '+snippet : '')));
    row.onclick = () => openNote(note.id); list.append(row);
  }
  if (!notes.length) list.append(text('p',query ? 'No notes match that phrase.' : 'Your next thought can start here.','muted'));
}
function rememberDraft() {
  if (!current) return;
  current = {...current, title:$('note-title').value, account:$('note-account').value, body:bat.snapshot().body};
  try { localStorage.setItem('bat-draft:'+current.id, JSON.stringify(current)); }
  catch { banner('The local recovery draft could not be written. Keep this window open until the note is saved.'); }
}
function changed() {
  if (!current) return;
  dirty = true; editSequence++; rememberDraft();
  $('save-status').textContent = 'Saving…'; clearTimeout(saveTimer); saveTimer = setTimeout(flush,400);
}
async function flush() {
  clearTimeout(saveTimer);
  if (saving) { await saving; if (dirty && !$('retry').hidden) return false; if (dirty) return flush(); return true; }
  if (!dirty || !current) return true;
  rememberDraft();
  const draft = {...current}, sequence = editSequence;
  saving = (async () => {
    try {
      const reply = await post('note.save',draft);
      const saved = reply.state.workspace.notes.find(n => n.id === draft.id);
      current.revision = saved.revision;
      if (sequence === editSequence) { dirty = false; localStorage.removeItem('bat-draft:'+draft.id); }
      else rememberDraft();
      $('retry').hidden = true; $('save-status').textContent = dirty ? 'Saving…' : 'Saved on this device';
      if (saveError && $('banner').textContent === saveError) banner('');
      saveError = '';
      renderNotes(); return true;
    } catch (e) {
      $('save-status').textContent = 'Not saved · draft kept on this device'; $('retry').hidden = false;
      saveError = e.message; banner(saveError); return false;
    } finally { saving = null; }
  })();
  const ok = await saving;
  if (ok && dirty) return flush(); return ok;
}
async function openNote(id) {
  if (!(await flush())) return false;
  const saved = state.workspace.notes.find(n=>n.id===id); if (!saved) return false;
  current = {...saved}; dirty = false;
  let recovered;
  try { recovered = JSON.parse(localStorage.getItem('bat-draft:'+id) || 'null'); } catch {}
  if (recovered?.id === id && recovered.revision === saved.revision) { current = {...saved,...recovered}; dirty = true; banner('Recovered an unsaved draft. Review it, then choose Retry save.'); }
  else if (recovered) { banner('An older recovery draft is preserved on this device. It has not overwritten the saved note.'); }
  $('note-title').value = current.title; $('note-account').value = current.account;
  $('empty-note').hidden = true; $('document').hidden = false;
  $('notes-workspace').classList.add('editing');
  $('save-status').textContent = dirty ? 'Recovered draft · not yet saved' : 'Saved on this device'; $('retry').hidden = !dirty;
  setView('notes'); await post('host.focus'); bat.open({id:current.id,body:current.body}); renderNotes();
  sessionStorage.setItem('bat-note',current.id); return true;
}
async function capture(kind) {
  if (!state || captureBusy) return;
  if (kind === 'reminder') {
    try { await post('host.focus'); } catch(e) { banner(e.message); return; }
    if (!$('reminder-dialog').open) {
      if (!$('reminder-due').value) { const date = new Date(Date.now()+30*60000); date.setSeconds(0,0); $('reminder-due').value = localDate(date); }
      $('reminder-dialog').showModal(); $('reminder-action').focus();
    } return;
  }
  captureBusy = true;
  try {
    if (!(await flush())) return;
    const previous = new Set(state.workspace.notes.map(n=>n.id));
    const result = await post('note.create',{account:''});
    await openNote(result.state.workspace.notes.find(n=>!previous.has(n.id)).id);
  } catch(e) { banner(e.message); } finally { captureBusy = false; }
}
function localDate(date) { return new Date(date.getTime()-date.getTimezoneOffset()*60000).toISOString().slice(0,16); }
function renderReminderQueue() {
  if (!state) return;
  const rows = [...state.workspace.reminders].sort((a,b)=>new Date(a.due)-new Date(b.due));
  const due = rows.filter(r=>new Date(r.due)<=new Date(state.now)).length;
  $('reminder-count').textContent = due+' due';
  $('queue-label').textContent = rows.length+' outstanding · due first, then upcoming';
  $('show-all').textContent = allReminders ? 'Show fewer' : 'Show all';
  const list = $('reminder-list'); list.replaceChildren();
  for (const r of rows.slice(0,allReminders ? rows.length : 4)) {
    const row = text('div','','reminder-row'); row.id='reminder-'+r.id;
    row.classList.toggle('due',new Date(r.due)<=new Date(state.now));
    const copy = text('div','','reminder-copy'); copy.append(text('strong',r.action));
    copy.append(text('small',(r.account || 'Unassigned')+' · '+new Date(r.due).toLocaleString(undefined,{month:'short',day:'numeric',hour:'2-digit',minute:'2-digit'})));
    row.append(copy); list.append(row);
  }
  list.style.maxHeight = allReminders ? '290px' : ''; list.style.overflowY = allReminders ? 'auto' : '';
  $('more-reminders').textContent = !allReminders && rows.length>4 ? '+'+(rows.length-4)+' more outstanding' : '';
  if (!rows.length) list.append(text('p','Nothing waiting for you. Add a reminder when something needs a second look.','muted'));
}
const examples = [{start:9,end:10.25,account:'Acme',title:'Press office',style:''},{start:10.5,end:11.5,account:'Beacon',title:'Launch planning',style:'beacon'},{start:12,end:12.75,account:'Northstar',title:'Strategy',style:'northstar'},{start:14,end:15.5,account:'Acme',title:'Client call',style:''}];
function renderTimeline() {
  const [from,to] = range==='morning' ? [8,13] : range==='afternoon' ? [12,18] : [8,18];
  $('axis').replaceChildren(...Array.from({length:6},(_,i)=>text('span',String(Math.round(from+(to-from)*i/5)).padStart(2,'0')+':00')));
  for (const name of ['work-track','meeting-track','reminder-track']) $(name).replaceChildren();
  function block(entry,track) {
    if (entry.end<=from || entry.start>=to) return;
    const el = text('button','','block '+entry.style);
    el.style.left = (Math.max(entry.start,from)-from)/(to-from)*100+'%';
    el.style.width = (Math.min(entry.end,to)-Math.max(entry.start,from))/(to-from)*100-.7+'%';
    el.append(text('strong',entry.account)); if (track==='work-track') el.append(text('small',entry.title));
    el.setAttribute('aria-label',entry.account+' '+entry.title+' example');
    el.onclick = () => { $('selection').textContent = entry.account+' · '+entry.title+' · '+time(entry.start)+'–'+time(entry.end)+' · Example only'; };
    $(track).append(el);
  }
  examples.forEach(e=>block(e,'work-track'));
  block({start:14,end:15,account:'Acme client call',title:'Meeting reservation',style:''},'meeting-track');
  for (const r of state?.workspace.reminders || []) {
    const d = new Date(r.due), now = new Date(state.now), hour = d.getHours()+d.getMinutes()/60;
    if (d.toDateString()!==now.toDateString()||hour<from||hour>to) continue;
    const marker = text('button','◆','marker'); marker.style.left=(hour-from)/(to-from)*100+'%'; marker.setAttribute('aria-label',r.action+' at '+time(hour));
    marker.onclick = () => { allReminders=true;renderReminderQueue(); const row=$('reminder-'+r.id);row?.classList.add('selected');row?.scrollIntoView({block:'nearest'}); };
    $('reminder-track').append(marker);
  }
}
function time(hour) { return String(Math.floor(hour)).padStart(2,'0')+':'+String(Math.round(hour%1*60)).padStart(2,'0'); }
function renderRecent() {
  const list = $('recent-work'); list.replaceChildren();
  for (const row of state.workspace.sessions.slice(-4).reverse()) {
    const el = text('div','','work-row'), name = text('div',row.account); name.append(text('small',row.date+' · '+row.start.slice(0,5)+'–'+row.end.slice(0,5)));
    el.append(name,text('span',row.minutes+' min')); list.append(el);
  }
  if (!state.workspace.sessions.length) list.append(text('p','Your recorded work will appear here. The timeline above shows fictional layout examples.','muted'));
}
// The same tested editor is hosted within the complete frame. Its messages stay local to this module.
window.invokeCSharpAction = body => {
  const message = JSON.parse(body);
  if (message.type==='changed' && message.id===current?.id) changed();
  if (message.type==='editLink') {
    linkReturn=document.activeElement;$('link-text').value=message.text;$('link-url').value=message.url;$('link-error').textContent='';$('link-dialog').showModal();$('link-url').focus();
  }
  if (message.type==='focusShell') $('note-title').focus();
  if (message.type==='compactWindow') notify('host.resize');
  if (message.type==='close') { flush(); $('note-title').focus(); }
};
$('timesheets').onclick=()=>{flush();setView('timesheets');};
$('notes').onclick=()=>setView('notes');
document.querySelector('.brand').onclick=e=>{e.preventDefault();setView('timesheets');};
for (const id of ['quick-note','new-note','empty-create','call-note']) $(id).onclick=()=>capture('note');
$('quick-reminder').onclick=()=>capture('reminder');
$('compact').onclick=async()=>{if(await flush()) notify('host.hide');};
$('timer-toggle').onclick=async()=>{try{await post('timer.toggle',{account:$('timer-account').value});}catch(e){banner(e.message);}};
$('note-title').oninput=changed;$('note-account').onchange=changed;$('retry').onclick=flush;
$('search').oninput=renderNotes;$('account-filter').onchange=renderNotes;
$('back-library').onclick=async()=>{if(await flush()) $('notes-workspace').classList.remove('editing');};
$('show-all').onclick=()=>{allReminders=!allReminders;renderReminderQueue();};
document.querySelectorAll('[data-range]').forEach(button=>button.onclick=()=>{range=button.dataset.range;document.querySelectorAll('[data-range]').forEach(b=>b.setAttribute('aria-pressed',String(b===button)));renderTimeline();});
$('cancel-reminder').onclick=()=>$('reminder-dialog').close();
$('reminder-form').onsubmit=async e=>{
  e.preventDefault(); const button=e.submitter;button.disabled=true;
  try { await post('reminder.create',{action:$('reminder-action').value,account:$('reminder-account').value,due:new Date($('reminder-due').value).toISOString()});$('reminder-dialog').close();$('reminder-action').value='';$('reminder-error').textContent=''; }
  catch(error){$('reminder-error').textContent=error.message;}finally{button.disabled=false;}
};
function closeLink() { $('link-dialog').close(); bat.focus(); }
$('cancel-link').onclick=closeLink;
$('link-dialog').addEventListener('cancel',()=>setTimeout(()=>bat.focus(),0));
$('link-form').onsubmit=e=>{e.preventDefault();let url;try{url=new URL($('link-url').value);if(!['https:','http:'].includes(url.protocol))throw Error();}catch{$('link-error').textContent='Use an http or https address.';return;}bat.insertLink($('link-text').value,url.href);closeLink();};
$('format-toggle').onclick=()=>{const visible=$('format-bar').hidden;$('format-bar').hidden=!visible;$('format-toggle').setAttribute('aria-pressed',String(visible));};
document.querySelectorAll('[data-format]').forEach(button=>{button.onmousedown=e=>e.preventDefault();button.onclick=()=>button.dataset.format==='heading'?bat.heading():bat.format(button.dataset.format);});
$('link-button').onmousedown=e=>e.preventDefault();$('link-button').onclick=()=>bat.editLink();
document.addEventListener('keydown',e=>{
  if(e.ctrlKey && e.shiftKey && e.key.toLowerCase()==='q') {e.preventDefault();notify('host.quit');}
  if(e.key==='F9' && !e.target.closest('#editor')) {e.preventDefault();notify('host.resize');}
  if(e.key==='F6' && !e.target.closest('#editor')) {e.preventDefault();bat.focus();}
});
window.addEventListener('beforeunload',e=>{if(dirty){rememberDraft();e.preventDefault();e.returnValue='';}});
window.batFrame = {flush, receive, capture};
setView(sessionStorage.getItem('bat-view')==='notes'?'notes':'timesheets');
if(window.chrome?.webview) window.chrome.webview.postMessage({version:1,requestId:crypto.randomUUID(),type:'host.ready',payload:{}});
else banner('This is the BAT development frame. Open it through the Windows host to save and record.');
