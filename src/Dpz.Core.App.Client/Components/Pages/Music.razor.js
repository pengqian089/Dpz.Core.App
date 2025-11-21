export function scrollToLyric(index) {
  const container = document.getElementById('lyrics-scroll');
  if (!container) return;
  
  const el = document.getElementById(`lyric-${index}`);
  if (!el) return;
  
  const containerHeight = container.clientHeight;
  const elementTop = el.offsetTop;
  const elementHeight = el.clientHeight;
  
  const targetScrollTop = elementTop - (containerHeight / 3) + (elementHeight / 2);
  
  container.scrollTo({ 
    top: Math.max(0, targetScrollTop), 
    behavior: 'smooth' 
  });
}

/**
 * 触发封面切换动画
 * @param {string} direction - 'next' 或 'previous'
 */
export function triggerCoverSwitchAnimation(direction) {
  const coverStack = document.querySelector('.cover-stack');
  if (!coverStack) return;

  const currentCover = coverStack.querySelector('.cover-stack-item.current');
  if (!currentCover) return;

  // 添加滑出动画类
  const animationClass = direction === 'next' ? 'switching-out-next' : 'switching-out-prev';
  currentCover.classList.add(animationClass);

  // 为下一个封面添加滑入动画
  const nextCover = coverStack.querySelector('.cover-stack-item.next');
  if (nextCover) {
    nextCover.classList.add('switching-in');
  }

  // 动画结束后清理类
  setTimeout(() => {
    currentCover.classList.remove(animationClass);
    if (nextCover) {
      nextCover.classList.remove('switching-in');
    }
  }, 500); // 与CSS动画时长匹配
}

/**
 * 初始化封面手势支持 - 优化版本
 * @param {Object} dotNetRef - .NET 对象引用
 */
export function initCoverGesture(dotNetRef) {
  const coverStack = document.querySelector('.cover-stack');
  if (!coverStack) {
    console.warn('Cover stack not found');
    return;
  }

  // 移除旧的事件监听器
  disposeCoverGesture();

  // 绑定新的启动事件
  coverStack.addEventListener('mousedown', (e) => handleGestureStart(e, dotNetRef));
  coverStack.addEventListener('touchstart', (e) => handleGestureStart(e, dotNetRef), { passive: true });
}

export function disposeCoverGesture() {
  const coverStack = document.querySelector('.cover-stack');
  if (!coverStack) return;

  // 移除所有相关监听器
  coverStack.removeEventListener('mousedown', handleGestureStart);
  coverStack.removeEventListener('touchstart', handleGestureStart);
  document.removeEventListener('mousemove', handleGestureMove);
  document.removeEventListener('touchmove', handleGestureMove);
  document.removeEventListener('mouseup', handleGestureEnd);
  document.removeEventListener('touchend', handleGestureEnd);
  document.removeEventListener('mouseleave', handleGestureEnd);
}

// --- 手势状态变量 ---
let isDragging = false;
let startX = 0;
let currentX = 0;
let animationFrameId = null;
let dotNetRef = null;
let currentCoverEl = null;
let nextCoverEl = null;
let coverWidth = 0;

function handleGestureStart(e, ref) {
  // 阻止在非当前封面上启动拖动
  if (!e.target.closest('.cover-stack-item.current')) return;
  
  dotNetRef = ref;
  isDragging = true;
  startX = e.type === 'touchstart' ? e.touches[0].clientX : e.clientX;
  
  currentCoverEl = document.querySelector('.cover-stack-item.current');
  nextCoverEl = document.querySelector('.cover-stack-item.next');
  if (!currentCoverEl) return;

  coverWidth = currentCoverEl.offsetWidth;

  // 移除过渡效果以便手动控制
  currentCoverEl.style.transition = 'none';
  if (nextCoverEl) {
    nextCoverEl.style.transition = 'none';
  }

  // 添加全局移动和结束监听器
  document.addEventListener('mousemove', handleGestureMove);
  document.addEventListener('touchmove', handleGestureMove, { passive: false });
  document.addEventListener('mouseup', handleGestureEnd);
  document.addEventListener('touchend', handleGestureEnd);
  document.addEventListener('mouseleave', handleGestureEnd); // 处理鼠标移出窗口
}

function handleGestureMove(e) {
  if (!isDragging) return;

  // 阻止页面滚动
  if (e.type === 'touchmove') {
    e.preventDefault();
  }

  currentX = e.type === 'touchmove' ? e.touches[0].clientX : e.clientX;
  
  // 使用 requestAnimationFrame 优化性能
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId);
  }
  animationFrameId = requestAnimationFrame(updateCoverPositions);
}

function updateCoverPositions() {
  if (!isDragging || !currentCoverEl) return;

  const deltaX = currentX - startX;
  const progress = Math.max(-1, Math.min(1, deltaX / coverWidth));

  // 移动当前封面
  currentCoverEl.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.05})`;
  currentCoverEl.style.opacity = `${1 - Math.abs(progress) * 0.3}`;

  // 联动下一张封面
  if (nextCoverEl && deltaX < 0) { // 只在向左滑时
    const nextProgress = Math.min(1, Math.abs(progress));
    const scale = 0.94 + 0.06 * nextProgress;
    const translateY = 12 * (1 - nextProgress);
    nextCoverEl.style.transform = `translateY(${translateY}px) scale(${scale})`;
    nextCoverEl.style.opacity = `${0.8 + 0.2 * nextProgress}`;
  }
}

async function handleGestureEnd(e) {
  if (!isDragging) return;
  isDragging = false;

  cancelAnimationFrame(animationFrameId);

  const deltaX = currentX - startX;
  const threshold = coverWidth * 0.3; // 滑动超过30%宽度触发切换

  // 恢复过渡效果
  if (currentCoverEl) {
    currentCoverEl.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
  }
  if (nextCoverEl) {
    nextCoverEl.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
  }

  if (Math.abs(deltaX) > threshold) {
    // 触发切换
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    if (currentCoverEl) {
      currentCoverEl.style.transform = `translateX(${deltaX > 0 ? '100%' : '-100%'}) scale(0.9)`;
      currentCoverEl.style.opacity = '0';
    }

    // [FIX] Check if dotNetRef is still valid before invoking
    if (dotNetRef) {
      try {
        if (direction === 'next') {
          await dotNetRef.invokeMethodAsync('OnSwipeNext');
        } else {
          await dotNetRef.invokeMethodAsync('OnSwipePrevious');
        }
      } catch (error) {
        console.error('Failed to invoke swipe handler:', error);
      }
    }
    
    // 动画结束后清理样式，以防影响下一次交互
    setTimeout(resetElementStyles, 300);

  } else {
    // 弹回原位
    resetElementStyles();
  }

  // 清理全局监听器
  document.removeEventListener('mousemove', handleGestureMove);
  document.removeEventListener('touchmove', handleGestureMove);
  document.removeEventListener('mouseup', handleGestureEnd);
  document.removeEventListener('touchend', handleGestureEnd);
  document.removeEventListener('mouseleave', handleGestureEnd);
  
  // 重置状态
  dotNetRef = null;
  currentCoverEl = null;
  nextCoverEl = null;
}

function resetElementStyles() {
  const current = document.querySelector('.cover-stack-item.current');
  const next = document.querySelector('.cover-stack-item.next');
  
  if (current) {
    current.style.transform = '';
    current.style.opacity = '';
  }
  if (next) {
    next.style.transform = '';
    next.style.opacity = '';
  }
}
