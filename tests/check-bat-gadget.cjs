const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),{chromium}=require('playwright');
const root=path.resolve(__dirname,'..'),source=fs.readFileSync(path.join(root,'prototypes/generated/bat-gadget.html'),'utf8');
(async()=>{const browser=await chromium.launch({headless:true,...(process.env.CHROME_PATH?{executablePath:process.env.CHROME_PATH}:{})});try{
  const page=await browser.newPage({viewport:{width:1024,height:1050}}),errors=[],requests=[];
  page.on('pageerror',e=>errors.push(e.message));page.on('request',r=>requests.push(r.url()));
  await page.addScriptTag({path:path.join(root,'prototypes/vendor/lucide-1.8.0.min.js')});
  await page.evaluate(()=>{window.saved=null;window.writes=0;window.openai={setWidgetState:async s=>{saved=structuredClone(s);writes++;}};});
  await page.setContent(source);assert.deepEqual(errors,[]);assert.equal(await page.evaluate(()=>writes),0);
  const action=async a=>{await page.locator('[data-gadget="'+a+'"]:visible').first().click();if(a==='reminder')await page.waitForFunction(()=>{const f=document.querySelector('[data-gadget-form]');return f.parentElement.getBoundingClientRect().height+1>=f.getBoundingClientRect().height;});},field=n=>page.locator('[data-gadget-field="'+n+'"]'),snap=()=>page.evaluate(()=>saved);
  assert.equal(await page.locator('[data-gadget-recent]>button').count(),3);assert.equal(await page.locator('[data-tree]').isVisible(),false);
  assert.equal(await page.locator('[data-gadget-tracking]').isVisible(),false);
  await action('workspace');assert.equal(await page.locator('.app').isVisible(),false);
  await action('reminder');await field('label').fill('Chase launch approval');await field('home').selectOption('Acme');await field('hour').fill('09');await field('minute').fill('07');
  // Six hour-wheel steps make 09:07 into 15:07 without repeated minute taps.
  await field('hour').hover();for(let i=0;i<6;i++){await page.mouse.wheel(0,-100);await page.waitForFunction(v=>document.querySelector('[data-gadget-field=hour]').value===v,String(10+i),{timeout:3000}).catch(async e=>{fs.mkdirSync(path.join(root,'artifacts/gadget'),{recursive:true});await page.screenshot({path:path.join(root,'artifacts/gadget/wheel-debug.png')});throw Error('Wheel hour='+await field('hour').inputValue()+'; '+errors.join(';'));});}
  await page.waitForFunction(()=>document.querySelector('[data-gadget-field=hour]').value==='15');assert.equal(await field('minute').inputValue(),'07');
  await field('minute').hover();await page.mouse.wheel(0,-100);await page.waitForFunction(()=>document.querySelector('[data-gadget-field=minute]').value==='08');
  await action('close');assert.equal(await page.locator('[data-gadget=reminder]').first().evaluate(e=>document.activeElement===e),true);await action('reminder');assert.equal(await field('label').inputValue(),'Chase launch approval');assert.equal(await field('hour').inputValue(),'15');
  await field('hour').fill('23');await field('minute').fill('59');await field('minute').press('ArrowUp');assert.equal(await field('day').inputValue(),'2026-11-21');assert.equal(await field('hour').inputValue(),'00');assert.equal(await field('minute').inputValue(),'00');await field('day').fill('2026-11-20');await field('hour').fill('15');await page.locator('[data-gadget-save]').click();let s=await snap();
  const reminder=s.privateContent.reminders.find(r=>r.id.startsWith('gadget-reminder-'));assert.equal(reminder.when,'2026-11-20T15:00');assert.equal(reminder.home,'Acme');assert.equal(reminder.noteId,undefined);assert.equal(s.privateContent.changes.length,0);assert.equal(s.privateContent.state.gadget.showApp,false);
  await action('tracking');await page.locator('#gadget-record-account').selectOption('Beacon');await page.locator('.gadget-host [data-action=timer]').click();
  assert.equal((await snap()).modelContent.recording,true);
  await action('reminder');await field('label').fill('Follow up after the call');await field('home').selectOption('Beacon');
  await action('note');const id=(await snap()).modelContent.selectedNote;
  const body=page.locator('[data-note-editor="'+id+'"] .toastui-editor-ww-container [contenteditable]');
  await page.waitForFunction(()=>document.activeElement?.closest('[data-note-editor]'));
  await page.keyboard.insertText('Neighbourhood launch decisions');await page.keyboard.press('Enter');await page.keyboard.insertText('Use the approved customer quote.');
  await page.waitForTimeout(400);assert.equal((await snap()).modelContent.recording,true);assert.equal((await snap()).privateContent.state.gadget.recordAccount,'Beacon');
  await action('day');
  assert((await page.locator('[data-gadget-recent]').textContent()).includes('Neighbourhood launch decisions'));
  await action('reminder');assert.equal(await field('label').inputValue(),'Follow up after the call');assert.equal(await field('home').inputValue(),'Beacon');await action('close');
  await page.locator('[data-gadget-recent] [data-open="'+id+'"]').click();assert((await body.innerText()).includes('approved customer quote'));await action('day');
  // Existing queue Move opens the attached drawer and preserves reminder identity/association.
  await page.locator('[data-queue] [data-edit-reminder]').first().click();assert(await page.locator('[data-gadget-form]').isVisible());const editing=(await snap()).privateContent.state.gadget.editId;
  const original=(await snap()).privateContent.reminders.find(r=>r.id===editing);await field('hour').fill('16');await field('minute').fill('00');await page.locator('[data-gadget-save]').click();s=await snap();const moved=s.privateContent.reminders.find(r=>r.id===editing);assert.equal(moved.noteId,original.noteId);assert.equal(moved.when,'2026-11-20T16:00');
  fs.mkdirSync(path.join(root,'artifacts/gadget'),{recursive:true});
  for(const width of [1024,736,600,420,320]){
    await page.setViewportSize({width,height:1100});await page.waitForTimeout(60);
    assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),'Overflow '+width);
    await action('reminder');assert(await field('label').isVisible());
    const box=await page.locator('[data-gadget-form]').boundingBox();assert(box.x>=0&&box.x+box.width<=width,'Drawer bounds '+width);
    await page.screenshot({path:path.join(root,'artifacts/gadget/drawer-'+width+'.png')});await action('close');
    if(width<=600){const queue=await page.locator('[data-queue]').boundingBox(),notes=await page.locator('.day-recent').boundingBox();assert(queue.y+queue.height<=notes.y,'Reminder priority '+width);}
  }
  await page.setViewportSize({width:1024,height:1100});await page.emulateMedia({colorScheme:'dark',reducedMotion:'reduce'});await action('reminder');assert.equal(await page.locator('.gadget-drawer').evaluate(e=>getComputedStyle(e).transitionDuration),'0s');await action('close');
  await page.screenshot({path:path.join(root,'artifacts/gadget/dashboard-dark.png')});
  // Restore the same saved snapshot without issuing a write or losing the capture draft.
  const saved=await snap(),restored=await browser.newPage({viewport:{width:420,height:1100}});restored.on('pageerror',e=>errors.push(e.message));
  await restored.addScriptTag({path:path.join(root,'prototypes/vendor/lucide-1.8.0.min.js')});await restored.evaluate(s=>{window.writes=0;window.openai={widgetState:s,setWidgetState:async()=>writes++};},saved);await restored.setContent(source);await restored.waitForTimeout(80);assert.equal(await restored.evaluate(()=>writes),0);
  await restored.locator('[data-gadget=reminder]').first().click();assert.equal(await restored.locator('[data-gadget-field=label]').inputValue(),'Follow up after the call');assert.equal(await restored.locator('#gadget-record-account').inputValue(),'Beacon');
  assert.deepEqual(errors,[]);assert.deepEqual(requests,[]);console.log('PASS: connected drawer capture/move, wheel hours/minutes, retained drafts, independent full note capture/reopening, recording account continuity, bounded queue, 320–1024 layouts, reduced motion and no-write restore. Native focus/placement are separate gates.');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
