let _ref; let _observer;
export function initArticleRead(dotnetRef){
  _ref = dotnetRef;
  observeCommentsSentinel();
}
export function disposeArticleRead(){
  if(_observer){ _observer.disconnect(); _observer = null; }
  _ref = null;
}
function observeCommentsSentinel(){
  const sentinel = document.getElementById('comments-sentinel');
  if(!sentinel || !_ref) return;
  _observer = new IntersectionObserver(entries=>{
    entries.forEach(e=>{ if(e.isIntersecting){ _ref.invokeMethodAsync('LoadNextCommentsPageJs').catch(()=>{}); } });
  },{ root:null, threshold:0.25 });
  _observer.observe(sentinel);
}

export function initViewer(selector) {
    const element = document.querySelector(selector);
    if (element) {
        const viewer = new Viewer(element, {
            inline: false,
            url(image) {
                return image.src;
            },
        });
    }
}
