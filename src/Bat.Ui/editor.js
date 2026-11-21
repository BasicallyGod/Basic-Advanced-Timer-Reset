'use strict';
// The shell owns persistence; messages carry stable document IDs and revisions.
const instances = new Map();
let activeEditorId = '', loading = false, revision = 0;
let editor = null;
let restoredSelection = null;
let linkSelection = null;
function send(message) { if (typeof invokeCSharpAction === 'function') invokeCSharpAction(JSON.stringify(message)); }
function snapshot() {
  return {id:activeEditorId,body:editor?.getMarkdown()||'',revision,selection:editor?.getSelection(),scroll:window.scrollY};
}
window.bat = {
  open(note) {
    if (activeEditorId === note.id && editor) { this.focus(); return; }
    loading=true;
    editor?.destroy(); instances.clear();
    activeEditorId=note.id; revision=note.revision||0;linkSelection=null;restoredSelection=null;
    const entry={view:null};instances.set(activeEditorId,entry);
    editor=new toastui.Editor({el:document.getElementById('editor'),initialValue:note.body,initialEditType:'wysiwyg',height:'auto',minHeight:'280px',hideModeSwitch:true,toolbarItems:[],usageStatistics:false,autofocus:false,plugins:[markdownRules],hooks:{addImageBlobHook:()=>false},events:{change(){if(loading)return;revision++;send({type:'changed',...snapshot()});}}});
    const writing=document.querySelector('.toastui-editor-ww-container [contenteditable]');
    writing?.setAttribute('aria-label','Markdown note body');
    writing?.setAttribute('role','textbox');writing?.setAttribute('aria-multiline','true');
    loading=false;
    this.focus();
  },
  snapshot,
  focus(){editor?.focus();if(restoredSelection){try{editor.setSelection(...restoredSelection)}catch{}restoredSelection=null;}document.querySelector('.toastui-editor-ww-container [contenteditable]')?.focus({preventScroll:true});},
  remember(){restoredSelection=snapshot().selection;return snapshot();},
  restore(position){restoredSelection=position.selection;window.scrollTo(0,position.scroll||0);this.focus();},
  format(command){editor?.exec(command);this.focus();},
  heading(){editor?.exec('heading',{level:2});this.focus();},
  editLink(text='',url=''){linkSelection=snapshot().selection;send({type:'editLink',id:activeEditorId,text:text||editor?.getSelectedText()||'',url});},
  insertLink(text,url){
    if(!/^https?:\/\//i.test(url))return;
    editor?.focus();
    if(linkSelection)editor.setSelection(...linkSelection);
    editor?.exec('addLink',{linkText:text,linkUrl:url});
    restoredSelection=null;linkSelection=null;editor?.focus();
  },
  plain(body){loading=true;editor?.setMarkdown(body,false);loading=false;},
  html(){return editor?.getHTML()||'';}
};
document.addEventListener('click',event=>{
  const anchor=event.target.closest('#editor a');if(!anchor)return;
  event.preventDefault();const view=instances.get(activeEditorId)?.view;
  if(view){const start=view.posAtDOM(anchor,0);editor.setSelection(start,start+anchor.textContent.length);}
  window.bat.editLink(anchor.textContent,anchor.getAttribute('href'));
});
document.addEventListener('paste',event=>{
  if(!event.target.closest('#editor'))return;
  const url=event.clipboardData?.getData('text/plain')?.trim();
  if(/^https?:\/\/\S+$/.test(url||'')&&editor?.getSelectedText()){
    event.preventDefault();event.stopImmediatePropagation();window.bat.insertLink(editor.getSelectedText(),url);
  }
},true);
document.addEventListener('keydown',event=>{
  if(!event.target.closest('#editor'))return;
  if(event.key==='F9'){event.preventDefault();send({type:'compactWindow',id:activeEditorId});}
  if(event.key==='F6'){event.preventDefault();window.bat.remember();send({type:'focusShell',id:activeEditorId});}
  if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();window.bat.editLink();}
  if(event.key==='Escape'){event.preventDefault();send({type:'close',...snapshot()});}
});
send({type:'ready'});
