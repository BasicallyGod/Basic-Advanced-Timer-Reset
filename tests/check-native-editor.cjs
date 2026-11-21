const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {chromium}=require('playwright');
const root=path.resolve(__dirname,'..');
(async()=>{
  const browser=await chromium.launch({headless:true,...(process.env.CHROME_PATH?{executablePath:process.env.CHROME_PATH}:{})});
  try{
    const page=await browser.newPage({viewport:{width:640,height:800}}),errors=[],requests=[];
    page.on('pageerror',e=>errors.push(e.message));page.on('request',r=>requests.push(r.url()));
    await page.setContent('<div id="editor"></div>');
    await page.evaluate(()=>{window.messages=[];window.invokeCSharpAction=s=>messages.push(JSON.parse(s));});
    await page.addStyleTag({path:path.join(root,'experiments/native-notes/Editor/editor.css')});
    for(const file of ['prototypes/vendor/toastui-editor-3.2.2.min.js','prototypes/src/editor-rules.js','experiments/native-notes/Editor/editor.js'])await page.addScriptTag({path:path.join(root,file)});
    const initial='## Client call\n\n> A launch people can join.\n\n- Confirm neighbourhood champions.\n- Chase approval Thursday.\n\n[Campaign research](https://example.com/research)';
    await page.evaluate(body=>bat.open({id:'note-a',body}),initial);
    assert.equal(await page.locator('.toastui-editor-ww-container h2').textContent(),'Client call');
    await page.locator('.toastui-editor-ww-container li').first().click();await page.keyboard.press('End');await page.keyboard.insertText(' for Beacon.');
    assert((await page.evaluate(()=>bat.snapshot().body)).includes('champions. for Beacon.'));
    await page.getByRole('link',{name:'Campaign research'}).click();
    const edit=await page.evaluate(()=>messages.findLast(m=>m.type==='editLink'));
    assert.equal(edit.url,'https://example.com/research');assert.equal(edit.id,'note-a');
    // A shell focus mistake or autosave snapshot must not replace the link's bookmark.
    await page.keyboard.press('Control+a');await page.evaluate(()=>bat.remember());
    await page.evaluate(()=>bat.insertLink('Campaign research','https://example.com/revised'));
    assert((await page.evaluate(()=>bat.snapshot().body)).includes('https://example.com/revised'),await page.evaluate(()=>JSON.stringify(bat.snapshot())));
    const body=await page.evaluate(()=>bat.snapshot().body);
    const selection=await page.evaluate(()=>bat.snapshot().selection);
    await page.keyboard.press('F6');
    assert.equal((await page.evaluate(()=>messages.at(-1))).type,'focusShell');
    assert.equal(await page.evaluate(()=>bat.snapshot().body),body);
    await page.evaluate(()=>bat.focus());
    assert.deepEqual(await page.evaluate(()=>bat.snapshot().selection),selection);
    await page.locator('.toastui-editor-ww-container .ProseMirror').click();await page.keyboard.press('Control+a');await page.keyboard.press('Control+b');
    assert.notEqual(await page.evaluate(()=>bat.snapshot().body),body);
    await page.keyboard.press('Control+z');assert.equal(await page.evaluate(()=>bat.snapshot().body),body);
    const position=await page.evaluate(()=>bat.remember());
    await page.evaluate(()=>bat.open({id:'note-b',body:''}));
    await page.evaluate(()=>bat.focus());
    await page.keyboard.insertText('Immediate capture');assert.equal(await page.evaluate(()=>bat.snapshot().body),'Immediate capture');
    await page.evaluate(p=>{bat.open({id:'note-a',body:p.body});bat.restore(p)},position);
    assert.equal(await page.evaluate(()=>bat.snapshot().body),body);
    const changes=await page.evaluate(()=>messages.filter(m=>m.type==='changed'));
    assert(changes.length>0&&changes.every(m=>m.id&&Number.isInteger(m.revision)));
    assert.deepEqual(errors,[]);assert.deepEqual(requests,[]);
    console.log('PASS: hosted editor component renders Markdown, amends a list, edits a source link, undoes formatting, preserves selection through F6 handoff, restores document context and captures immediately without network requests. Native focus/accessibility/scaling are separate checks.');
  }finally{await browser.close()}
})().catch(e=>{console.error(e);process.exitCode=1});
