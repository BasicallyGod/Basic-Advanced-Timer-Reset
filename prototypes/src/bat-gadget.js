// Focused presentation comparison; uses the existing note identity, editor and day records.
let gadget=state.gadget??={showApp:true,showTimer:false,drawer:false,editId:null,recordAccount:'Acme',elapsed:0,
  captureDraft:{label:'',home:'',day:'2026-11-20',hour:'15',minute:'00'},editDraft:null,allDue:false};
let returnTo=null,timerStarted=0;
const gadgetHost=document.createElement('section');gadgetHost.className='gadget-host';gadgetHost.setAttribute('aria-label','BAT desktop gadget');
gadgetHost.innerHTML='<div class="gadget-bar"><button type="button" class="quiet gadget-brand" data-gadget="workspace" aria-label="Show or hide workspace">BAT</button><time class="gadget-clock" aria-label="Current preview time">10:45</time><button type="button" data-gadget="reminder" class="primary" aria-expanded="false" aria-controls="gadget-reminder-drawer">'+icon('bell-plus')+'Reminder</button><button type="button" class="quiet" data-gadget="note" aria-label="Quick note">'+icon('square-pen')+'<span>Quick note</span></button><button type="button" class="quiet gadget-count" data-gadget="queue" aria-label="Open due reminders">'+icon('bell')+'<span data-due></span></button><button type="button" class="quiet" data-gadget="tracking" aria-label="Show recording controls" aria-pressed="false">'+icon('timer')+'</button></div><div class="gadget-tracking" data-gadget-tracking hidden><label class="sr" for="gadget-record-account">Recording account</label><select id="gadget-record-account">'+accounts.map(a=>'<option>'+esc(a)+'</option>').join('')+'</select><span data-timer class="timer-readout"></span><button type="button" data-action="timer" aria-label="Start timer">'+icon('play')+'</button></div><div class="gadget-drawer" data-gadget-drawer><div><form id="gadget-reminder-drawer" data-gadget-form hidden aria-label="Attached reminder drawer"><div class="drawer-heading"><h3 data-gadget-heading>Remind me</h3><button type="button" class="quiet" data-gadget="close" aria-label="Close reminder drawer">'+icon('x')+'</button></div><label>Action<input data-gadget-field="label" placeholder="Chase approval for the launch" required autocomplete="off"></label><label>Account<select data-gadget-field="home"><option value="">Unassigned</option>'+accounts.map(a=>'<option>'+esc(a)+'</option>').join('')+'</select></label><div class="gadget-schedule"><label>Day<input type="date" data-gadget-field="day" required></label><div><span class="muted">Time</span><div class="gadget-time" data-tooltip="Scroll hours or minutes; type for an exact time"><label class="sr" for="gadget-hour">Reminder hour</label><input id="gadget-hour" data-gadget-field="hour" type="text" inputmode="numeric" aria-label="Reminder hour"><span>:</span><label class="sr" for="gadget-minute">Reminder minute</label><input id="gadget-minute" data-gadget-field="minute" type="text" inputmode="numeric" aria-label="Reminder minute"></div></div></div><p class="error" data-gadget-error role="alert" hidden></p><div class="drawer-footer"><button type="button" class="quiet" data-gadget="close">Cancel</button><button type="submit" class="primary" data-gadget-save>Set reminder</button></div></form></div></div><span class="gadget-feedback sr" role="status" data-gadget-feedback></span>';
q('.pill').replaceWith(gadgetHost);
const recentCards=document.createElement('div');recentCards.className='gadget-note-cards';recentCards.dataset.gadgetRecent='';
q('.day-recent').appendChild(recentCards);q('.day-recent h3').textContent='Recent notes';q('[data-last-entry]').hidden=true;q('[data-recent-work]').hidden=true;q('[data-day-entries]').hidden=true;
const returnButton=document.createElement('button');returnButton.type='button';returnButton.dataset.gadget='day';returnButton.textContent='Back to day';returnButton.className='quiet';q('.writing-tools').prepend(returnButton);
const draft=()=>gadget.editId?gadget.editDraft:gadget.captureDraft;
function readGadgetDraft(){for(const input of qa('[data-gadget-field]'))draft()[input.dataset.gadgetField]=input.value;}
function paintGadget(){
  if(state.running&&!timerStarted)timerStarted=Date.now();
  q('.app').hidden=!gadget.showApp;
  gadgetHost.dataset.open=String(gadget.drawer);q('[data-gadget-form]').hidden=!gadget.drawer;
  q('[data-gadget=reminder]').setAttribute('aria-expanded',String(gadget.drawer));
  q('[data-gadget=tracking]').setAttribute('aria-pressed',String(gadget.showTimer));
  q('[data-gadget=tracking]').setAttribute('aria-label',gadget.showTimer?'Hide recording controls':'Show recording controls');
  q('[data-gadget-tracking]').hidden=!gadget.showTimer;
  q('#gadget-record-account').value=gadget.recordAccount;q('#gadget-record-account').disabled=state.running;
  const elapsed=gadget.elapsed+(state.running&&timerStarted?Math.floor((Date.now()-timerStarted)/1000):0);
  q('[data-timer]').textContent=(state.running?'Recording · ':'Paused · ')+String(Math.floor(elapsed/60)).padStart(2,'0')+':'+String(elapsed%60).padStart(2,'0');
  // When the optional panel is tucked away, active recording still has a visible cue.
  q('[data-gadget=tracking]').classList.toggle('recording-active',state.running);
  q('[data-gadget=tracking]').setAttribute('data-tooltip',state.running?'Recording '+gadget.recordAccount:'Optional recording controls');
  q('[data-gadget-heading]').textContent=gadget.editId?'Move reminder':'Remind me';
  q('[data-gadget-save]').textContent=gadget.editId?'Move':'Set reminder';
  for(const input of qa('[data-gadget-field]'))if(document.activeElement!==input)input.value=draft()?.[input.dataset.gadgetField]??'';
  const due=reminders.filter(r=>!r.done&&r.when<='2026-11-20T10:45').sort((a,b)=>a.when.localeCompare(b.when));
  q('[data-due]').textContent=due.length;
  const selected=due.some(r=>r.id===state.selectedReminder)?state.selectedReminder:due[0]?.id;
  q('[data-queue]').innerHTML='<div class="gadget-queue-heading"><h3>Reminders <span class="muted">'+due.length+' due</span></h3><button type="button" class="quiet" data-gadget="reminder" aria-label="New reminder">'+icon('plus')+'</button></div>'+due.slice(0,gadget.allDue?due.length:3).map(r=>'<div class="queue-row"><button type="button" data-select-reminder="'+r.id+'">'+esc(r.label)+'</button><span class="muted">'+esc(r.home||'Unassigned')+' · '+(r.when.slice(0,10)==='2026-11-20'?r.when.slice(11):'Yesterday')+'</span>'+(r.id===selected?'<div class="row"><button type="button" data-edit-reminder="'+r.id+'">Move</button><button type="button" data-done="'+r.id+'">Done</button></div>':'')+'</div>').join('')+(due.length>3?'<button type="button" class="quiet" data-gadget="more-due">'+(gadget.allDue?'Show fewer':(due.length-3)+' more due')+'</button>':'')+(!due.length?'<p class="muted">Nothing due. Your upcoming reminders stay on the timeline.</p>':'');
  const recent=[...notes].filter(n=>n.body.trim()).sort((a,b)=>b.updated.localeCompare(a.updated)).slice(0,3);
  recentCards.innerHTML=recent.map((n,i)=>{const lines=plain(n.body).split('\n').filter(s=>s.trim()),quote=lines.find(s=>/[“”]/.test(s)),snippet=quote||lines.slice(1).find(s=>s.length>25)||lines[0]||'';return '<button type="button" class="gadget-note-card '+(quote?'has-quote':'')+'" data-open="'+n.id+'"><span class="note-context">'+icon(quote?'quote':'file-text')+esc(n.home||'Unassigned')+' · '+dayLabel(n)+'</span><strong>'+esc(n.title||'Untitled note')+'</strong><span class="note-excerpt">'+esc(snippet.slice(0,170))+'</span><span class="note-open">Open note '+icon('arrow-up-right')+'</span></button>';}).join('');icons();
}
const baseGadgetRender=render;
render=function(){Object.assign(gadget,state.gadget||{});state.gadget=gadget;baseGadgetRender();paintGadget();};
function returnGadgetFocus(){(returnTo?.isConnected?returnTo:q('[data-gadget=reminder]')).focus();}
function closeGadgetDrawer(){readGadgetDraft();gadget.drawer=false;paintGadget();persist();returnGadgetFocus();}
function openGadgetReminder(id=null,fromNote=false){
  if(gadget.drawer)readGadgetDraft();
  returnTo=document.activeElement;gadget.editId=id;
  if(id){const r=reminders.find(r=>r.id===id);if(!r)return;gadget.editDraft={label:r.label,home:r.home||'',day:r.when.slice(0,10),hour:r.when.slice(11,13),minute:r.when.slice(14,16)};}
  else if(fromNote){gadget.captureDraft={label:editor()?.getSelectedText()?.trim()||current().title,home:current().home,day:'2026-11-20',hour:'15',minute:'00',noteId:current().id};}
  gadget.drawer=true;q('[data-gadget-error]').hidden=true;paintGadget();persist();q('[data-gadget-field=label]').focus();
}
openReminder=function(id=null){openGadgetReminder(id,!id&&state.workspace==='notes');};
function gadgetStep(field,delta){
  readGadgetDraft();const d=draft(),h=Number(d.hour),m=Number(d.minute);
  if(!Number.isInteger(h)||!Number.isInteger(m)||h<0||h>23||m<0||m>59)return;
  const date=new Date(d.day+'T00:00:00Z');if(!Number.isFinite(date.getTime()))return;
  date.setUTCMinutes(h*60+m+(field==='hour'?delta*60:delta));
  d.day=date.toISOString().slice(0,10);d.hour=String(date.getUTCHours()).padStart(2,'0');d.minute=String(date.getUTCMinutes()).padStart(2,'0');
  for(const input of qa('[data-gadget-field]'))input.value=d[input.dataset.gadgetField];persist();
}
root.addEventListener('click',e=>{
  const button=e.target.closest('[data-gadget],[data-action=timer]');if(!button)return;
  const action=button.dataset.gadget||'timer';
  if(action==='reminder')openGadgetReminder();
  else if(action==='close')closeGadgetDrawer();
  else if(action==='workspace'){gadget.showApp=!gadget.showApp;paintGadget();persist();}
  else if(action==='note'){if(gadget.drawer){readGadgetDraft();gadget.drawer=false;}gadget.showApp=true;closeDialog(false);newNote(true);}
  else if(action==='day'){flush();state.workspace='timesheets';render();persist();}
  else if(action==='tracking'){gadget.showTimer=!gadget.showTimer;paintGadget();persist();}
  else if(action==='timer'){if(state.running)gadget.elapsed+=Math.floor((Date.now()-timerStarted)/1000);state.running=!state.running;timerStarted=state.running?Date.now():0;render();persist();}
  else if(action==='queue'){gadget.showApp=true;state.workspace='timesheets';render();persist();q('[data-queue]').scrollIntoView({block:'nearest'});}
  else if(action==='more-due'){gadget.allDue=!gadget.allDue;paintGadget();persist();}
  else return;
  e.preventDefault();e.stopImmediatePropagation();
},true);
root.addEventListener('input',e=>{if(e.target.matches('[data-gadget-field]')){readGadgetDraft();persist();}});
root.addEventListener('change',e=>{if(e.target.id==='gadget-record-account'){gadget.recordAccount=e.target.value;persist();}});
root.addEventListener('submit',e=>{
  if(!e.target.matches('[data-gadget-form]'))return;e.preventDefault();readGadgetDraft();
  const d=draft(),hour=Number(d.hour),mins=Number(d.minute),when=d.day+'T'+String(hour).padStart(2,'0')+':'+String(mins).padStart(2,'0');
  if(!d.label.trim()||!/^\d{4}-\d{2}-\d{2}$/.test(d.day)||!/^\d{1,2}$/.test(d.hour)||!/^\d{1,2}$/.test(d.minute)||hour>23||mins>59||when<='2026-11-20T10:45'){q('[data-gadget-error]').textContent='Add an action and choose a future time.';q('[data-gadget-error]').hidden=false;return;}
  const existing=reminders.find(r=>r.id===gadget.editId);
  if(existing){undoMove={id:existing.id,when:existing.when};Object.assign(existing,{label:d.label.trim(),home:d.home,when});}
  else{reminders.push({id:'gadget-reminder-'+Date.now(),label:d.label.trim(),home:d.home,when,done:false,...(d.noteId?{noteId:d.noteId}:{})});gadget.captureDraft={label:'',home:'',day:d.day,hour:d.hour,minute:d.minute};}
  gadget.drawer=false;render();persist();q('[data-gadget-feedback]').textContent=(existing?'Moved':'Reminder set')+' for '+when.slice(11);say((existing?'Moved':'Reminder set')+' · '+when.slice(11));returnGadgetFocus();
});
root.addEventListener('wheel',e=>{const field=e.target.dataset.gadgetField;if(!['hour','minute'].includes(field))return;e.preventDefault();gadgetStep(field,e.deltaY<0?1:-1);},{passive:false});
root.addEventListener('keydown',e=>{
  if(e.key==='Escape'&&gadget.drawer){e.preventDefault();e.stopImmediatePropagation();closeGadgetDrawer();}
  const field=e.target.dataset.gadgetField;if(['hour','minute'].includes(field)&&['ArrowUp','ArrowDown'].includes(e.key)){e.preventDefault();gadgetStep(field,e.key==='ArrowUp'?1:-1);}
},true);
root.addEventListener('focusout',e=>{const f=e.target.dataset.gadgetField;if(['hour','minute'].includes(f)&&/^\d{1,2}$/.test(e.target.value)){e.target.value=e.target.value.padStart(2,'0');readGadgetDraft();persist();}});
if(globalThis.Tweak){const t=new Tweak({container:gadgetHost,onChange:()=>{paintGadget();persist();}});t.addToggle(gadget,'showTimer',{label:'Recording controls'});}
const gadgetTick=setInterval(()=>{if(!root.isConnected){clearInterval(gadgetTick);return;}if(!state.running)return;const elapsed=gadget.elapsed+Math.floor((Date.now()-timerStarted)/1000);q('[data-timer]').textContent='Recording · '+String(Math.floor(elapsed/60)).padStart(2,'0')+':'+String(elapsed%60).padStart(2,'0');},1000);
