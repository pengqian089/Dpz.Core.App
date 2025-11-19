let currentIndex=-1; let images=[]; let modal;

export function initMarkdownViewer(){
  // 事件代理点击图片
  document.addEventListener('click', e=>{
    const img = e.target.closest('.md-img');
    if(!img) return;
    images = Array.from(document.querySelectorAll('.md-img'));
    currentIndex = images.indexOf(img);
    showModal();
  });
}

function showModal(){
  if(currentIndex<0 || !images.length) return;
  const src = images[currentIndex].getAttribute('src');
  modal = document.createElement('div');
  modal.className='md-img-modal';
  modal.innerHTML = `
    <span class="close">?</span>
    <button class="nav-btn prev">?</button>
    <img src="${src}" alt="preview" />
    <button class="nav-btn next">?</button>
  `;
  document.body.appendChild(modal);
  modal.querySelector('.close').onclick=closeModal;
  modal.querySelector('.prev').onclick=()=>{ navigate(-1); };
  modal.querySelector('.next').onclick=()=>{ navigate(1); };
  modal.onclick = e=>{ if(e.target===modal) closeModal(); };
  window.addEventListener('keydown', onKey);
}
function navigate(dir){
  currentIndex = (currentIndex + dir + images.length) % images.length;
  const imgEl = modal.querySelector('img');
  imgEl.src = images[currentIndex].getAttribute('src');
}
function closeModal(){ if(modal){ modal.remove(); modal=null; images=[]; currentIndex=-1; window.removeEventListener('keydown', onKey); } }
function onKey(e){ if(!modal) return; if(e.key==='Escape') closeModal(); else if(e.key==='ArrowLeft') navigate(-1); else if(e.key==='ArrowRight') navigate(1); }

export function disposeMarkdownViewer(){ closeModal(); }
