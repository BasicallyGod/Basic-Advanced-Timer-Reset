const fs=require('node:fs'),path=require('node:path'),http=require('node:http'),assert=require('node:assert/strict');
const {chromium}=require('playwright');
const root=path.resolve(__dirname,'..');
const server=http.createServer((req,res)=>{
  const name=new URL(req.url,'http://localhost').pathname;
  const mapped=name==='/vendor/editor.js'?'prototypes/vendor/toastui-editor-3.2.2.min.js':name==='/editor-rules.js'?'prototypes/src/editor-rules.js':'src/Bat.Ui/'+(name==='/'?'index.html':name.slice(1));
  const file=path.resolve(root,mapped);
  if(!file.startsWith(root+path.sep)||!fs.existsSync(file)){res.writeHead(404);return res.end();}
  res.setHeader('Content-Type',file.endsWith('.css')?'text/css':file.endsWith('.js')?'text/javascript':'text/html');res.end(fs.readFileSync(file));
});
(async()=>{
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const browser=await chromium.launch({headless:true,...(process.env.CHROME_PATH?{executablePath:process.env.CHROME_PATH}:{})});
  try{
    const page=await browser.newPage({viewport:{width:1100,height:850}}),errors=[];
    page.on('pageerror',e=>errors.push(e.message));
    await page.addInitScript(()=>{
      let receive;
      window.commands=JSON.parse(sessionStorage.getItem('test-commands')||'[]');window.failSave=false;
      const snapshot=()=>JSON.parse(localStorage.getItem('test-host')||'null')||{workspace:{format:1,revision:0,notes:[],reminders:[],sessions:[],active:null,lastSeen:null},activeAccount:null,elapsedSeconds:0,now:'2026-11-21T10:45:00',notice:null};
      window.chrome={webview:{addEventListener:(name,handler)=>receive=handler,postMessage:async request=>{
        commands.push(request);sessionStorage.setItem('test-commands',JSON.stringify(commands));const state=snapshot();let ok=true,error=null;
        const p=request.payload;
        if(request.type==='host.ready'){receive({data:{type:'state',payload:state}});return;}
        if(request.type==='note.create')state.workspace.notes.unshift({id:crypto.randomUUID(),revision:0,title:'',account:p.account,body:'',updated:new Date().toISOString()});
        if(request.type==='note.save'){
          const note=state.workspace.notes.find(n=>n.id===p.id);
          if(failSave){ok=false;error='Simulated disk failure';}
          else if(note.revision!==p.revision){ok=false;error='Stale revision';}
          else Object.assign(note,p,{revision:p.revision+1,updated:new Date().toISOString()});
        }
        if(request.type==='reminder.create')state.workspace.reminders.push({...p,id:crypto.randomUUID()});
        if(request.type==='timer.toggle'){state.activeAccount=state.activeAccount?null:p.account;state.elapsedSeconds=state.activeAccount?12:0;}
        if(request.type.startsWith('host.') && request.type!=='host.focus')return;
        localStorage.setItem('test-host',JSON.stringify(state));
        await new Promise(resolve=>setTimeout(resolve,20));
        receive({data:{type:'reply',payload:{requestId:request.requestId,ok,state,error}}});
      }}};
    });
    await page.goto('http://127.0.0.1:'+server.address().port+'/');
    await page.getByRole('button',{name:'Start recording',exact:true}).click();
    await page.getByRole('button',{name:'+ Reminder',exact:false}).click();
    await page.getByLabel('What needs your attention?').fill('Chase campaign approval');
    await page.getByLabel('Account',{exact:true}).selectOption('Beacon');
    await page.getByLabel('Remind me at').fill('2026-11-21T15:00');
    await page.getByRole('button',{name:'Set reminder',exact:true}).click();
    await page.getByRole('button',{name:'Quick note',exact:false}).click();
    const writing=page.getByRole('textbox',{name:'Markdown note body'});
    await page.waitForFunction(()=>document.activeElement===document.querySelector('.toastui-editor-ww-container [contenteditable]'));
    await page.keyboard.insertText('Neighbourhood launch phrase');
    await page.getByLabel('Note account',{exact:true}).selectOption('Acme');
    await page.getByLabel('Note title',{exact:true}).fill('Client call');
    await page.waitForFunction(()=>document.getElementById('save-status').textContent==='Saved on this device');
    const firstId=await page.evaluate(()=>JSON.parse(localStorage.getItem('test-host')).workspace.notes[0].id);
    assert.equal(await page.getByRole('button',{name:'Pause recording',exact:true}).count(),1);
    await page.getByRole('button',{name:'Timesheets',exact:true}).click();
    assert.equal(await page.getByText('Chase campaign approval',{exact:true}).count(),1);
    await page.getByRole('button',{name:'Notes',exact:true}).click();
    await page.getByLabel('Search notes',{exact:true}).fill('Neighbourhood');
    await page.getByLabel('Filter notes by account').selectOption('Acme');
    await page.locator('.note-row').click();
    assert.equal(await writing.innerText(),'Neighbourhood launch phrase');
    await writing.click();await page.keyboard.press('End');await page.keyboard.insertText(' revised');
    await page.keyboard.press('Control+k');
    await page.getByLabel('Address',{exact:true}).fill('https://example.com/research');
    await page.getByLabel('Text',{exact:true}).fill('Campaign research');
    await page.getByRole('button',{name:'Apply',exact:true}).click();
    await page.waitForFunction(()=>document.getElementById('save-status').textContent==='Saved on this device');
    assert((await page.evaluate(()=>bat.snapshot().body)).includes('https://example.com/research'));
    await page.evaluate(()=>{failSave=true;});
    await writing.click();await page.keyboard.press('Control+End');await page.keyboard.insertText(' recovery draft');
    await page.getByRole('button',{name:'Retry save',exact:true}).waitFor();
    assert((await page.locator('#save-status').textContent()).includes('Not saved'));
    assert((await page.evaluate(id=>JSON.parse(localStorage.getItem('bat-draft:'+id)).body,firstId)).includes('recovery draft'));
    await page.evaluate(()=>{failSave=false;});await page.getByRole('button',{name:'Retry save',exact:true}).click();
    await page.waitForFunction(()=>document.getElementById('save-status').textContent==='Saved on this device');
    await page.reload();await page.getByRole('button',{name:'Notes',exact:true}).click();await page.locator('.note-row').click();
    assert((await writing.innerText()).includes('recovery draft'));
    assert.equal(await page.evaluate(()=>JSON.parse(localStorage.getItem('test-host')).workspace.notes[0].id),firstId);
    for(const width of [600,420]){
      await page.setViewportSize({width,height:800});
      assert.equal(await writing.isVisible(),true);
      assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),'page overflow at '+width);
      await page.getByRole('button',{name:'← Library',exact:true}).click();
      assert.equal(await page.getByLabel('Search notes',{exact:true}).isVisible(),true);
      await page.locator('.note-row').click();
    }
    const commands=await page.evaluate(()=>commands);
    assert(commands.filter(c=>c.type==='note.save').every(c=>c.payload.id===firstId));
    assert.equal(commands.filter(c=>c.type==='timer.toggle').length,1);
    assert.deepEqual(errors,[]);
    console.log('PASS: complete frame direct reminder/note capture, body/account retrieval, link editing, failure draft/retry, stable restart identity, independent recording and 600/420px layouts. Host is mocked; native evidence remains separate.');
  } finally {await browser.close();server.close();}
})().catch(e=>{console.error(e);server.close();process.exitCode=1;});
