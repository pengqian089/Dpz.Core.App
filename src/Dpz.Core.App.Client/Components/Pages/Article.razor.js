// JS Isolation for Article page
export function init(dotnetRef){
    setupPullToRefresh(dotnetRef);
    setupInfiniteScroll(dotnetRef);
}

export function dispose(){
    teardownPullToRefresh();
}

let _dotnetRef; let startY=0; let isPulling=false; const threshold=70; const body=document.body;
function setupPullToRefresh(ref){
    _dotnetRef = ref;
    body.addEventListener('touchstart', onTouchStart,{passive:true});
    body.addEventListener('touchmove', onTouchMove,{passive:true});
    body.addEventListener('touchend', onTouchEnd,{passive:true});
}
function teardownPullToRefresh(){
    body.removeEventListener('touchstart', onTouchStart);
    body.removeEventListener('touchmove', onTouchMove);
    body.removeEventListener('touchend', onTouchEnd);
    _dotnetRef = null;
}
function onTouchStart(e){ if(window.scrollY===0){ startY = e.touches[0].clientY; isPulling=true; } }
function onTouchMove(e){ if(!isPulling) return; const diff=e.touches[0].clientY - startY; if(diff>threshold){ triggerRefresh(); isPulling=false; } }
function onTouchEnd(){ isPulling=false; }
function triggerRefresh(){ if(_dotnetRef){ _dotnetRef.invokeMethodAsync('OnPullToRefresh'); } }

function setupInfiniteScroll(ref){
    const sentinel = document.getElementById('infinite-scroll-sentinel');
    if(!sentinel) return;
    const observer = new IntersectionObserver(entries=>{
        entries.forEach(entry=>{ if(entry.isIntersecting){ ref.invokeMethodAsync('LoadNextPageAsyncJs').catch(()=>{}); } });
    },{ root:null, threshold:0.1 });
    observer.observe(sentinel);
}
