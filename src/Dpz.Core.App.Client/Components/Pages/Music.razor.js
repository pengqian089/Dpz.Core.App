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

  const currentCover = coverStack.querySelector('.cover-stack-item.current');
  if (!currentCover) {
    console.warn('Current cover not found');
    return;
  }

  // 移除旧的事件监听器
  currentCover.removeEventListener('touchstart', handleTouchStart);
  currentCover.removeEventListener('touchmove', handleTouchMove);
  currentCover.removeEventListener('touchend', handleTouchEnd);

  // 添加触摸事件
  currentCover.addEventListener('touchstart', handleTouchStart, { passive: true });
  currentCover.addEventListener('touchmove', handleTouchMove, { passive: false });
  currentCover.addEventListener('touchend', (e) => handleTouchEnd(e, dotNetRef), { passive: true });

  // 添加鼠标事件
  currentCover.addEventListener('mousedown', handleMouseDown);
  currentCover.addEventListener('mousemove', handleMouseMove);
  currentCover.addEventListener('mouseup', (e) => handleMouseUp(e, dotNetRef));
  currentCover.addEventListener('mouseleave', resetSwipe);
}

export function disposeCoverGesture() {
  const currentCover = document.querySelector('.cover-stack-item.current');
  if (!currentCover) return;

  currentCover.removeEventListener('touchstart', handleTouchStart);
  currentCover.removeEventListener('touchmove', handleTouchMove);
  currentCover.removeEventListener('touchend', handleTouchEnd);
  currentCover.removeEventListener('mousedown', handleMouseDown);
  currentCover.removeEventListener('mousemove', handleMouseMove);
  currentCover.removeEventListener('mouseup', handleMouseUp);
  currentCover.removeEventListener('mouseleave', resetSwipe);
}

let touchStartX = 0;
let touchStartY = 0;
let touchEndX = 0;
let touchEndY = 0;
let isSwiping = false;

function handleTouchStart(e) {
  touchStartX = e.touches[0].clientX;
  touchStartY = e.touches[0].clientY;
  isSwiping = false;
}

function handleTouchMove(e) {
  if (!isSwiping) {
    const deltaX = Math.abs(e.touches[0].clientX - touchStartX);
    const deltaY = Math.abs(e.touches[0].clientY - touchStartY);
    
    if (deltaX > 10 && deltaX > deltaY) {
      isSwiping = true;
      e.preventDefault();
    }
  }
  
  if (isSwiping) {
    e.preventDefault();
    const coverStack = document.querySelector('.cover-stack');
    const currentCover = e.currentTarget;
    const deltaX = e.touches[0].clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / coverStack.offsetWidth));
    
    currentCover.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.15})`;
    currentCover.style.opacity = `${1 - Math.abs(progress) * 0.4}`;
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover && deltaX < 0) {
      const nextProgress = Math.min(1, Math.abs(progress));
      nextCover.style.transform = `translateZ(-${30 * (1 - nextProgress)}px) translateY(${15 * (1 - nextProgress)}px) scale(${0.92 + 0.08 * nextProgress})`;
      nextCover.style.opacity = `${0.75 + 0.25 * nextProgress}`;
    }
  }
}

async function handleTouchEnd(e, dotNetRef) {
  if (!isSwiping) {
    resetSwipe(e);
    return;
  }

  touchEndX = e.changedTouches[0].clientX;
  touchEndY = e.changedTouches[0].clientY;
  
  const coverStack = document.querySelector('.cover-stack');
  const currentCover = e.currentTarget;
  const deltaX = touchEndX - touchStartX;
  const threshold = coverStack.offsetWidth * 0.2; // 降低阈值到20%

  if (Math.abs(deltaX) > threshold) {
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    currentCover.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    currentCover.style.transform = `translateX(${deltaX > 0 ? '120%' : '-120%'}) scale(0.7)`;
    currentCover.style.opacity = '0';
    
    try {
      if (direction === 'next') {
        await dotNetRef.invokeMethodAsync('OnSwipeNext');
      } else {
        await dotNetRef.invokeMethodAsync('OnSwipePrevious');
      }
    } catch (error) {
      console.error('Failed to invoke swipe handler:', error);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
      resetSwipe({ currentTarget: currentCover });
    }, 100);
  } else {
    currentCover.style.transition = 'transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.3s ease';
    resetSwipe(e);
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover) {
      nextCover.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
      setTimeout(() => {
        nextCover.style.transition = '';
      }, 300);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
    }, 300);
  }
  
  isSwiping = false;
}

function handleMouseDown(e) {
  touchStartX = e.clientX;
  touchStartY = e.clientY;
  isSwiping = false;
}

function handleMouseMove(e) {
  if (e.buttons !== 1) return;
  
  if (!isSwiping) {
    const deltaX = Math.abs(e.clientX - touchStartX);
    const deltaY = Math.abs(e.clientY - touchStartY);
    
    // 降低触发阈值
    if (deltaX > 5 && deltaX > deltaY) {
      isSwiping = true;
    }
  }
  
  if (isSwiping) {
    const coverStack = document.querySelector('.cover-stack');
    const currentCover = e.currentTarget;
    const deltaX = e.clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / coverStack.offsetWidth));
    
    currentCover.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.15})`;
    currentCover.style.opacity = `${1 - Math.abs(progress) * 0.4}`;
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover && deltaX < 0) {
      const nextProgress = Math.min(1, Math.abs(progress));
      nextCover.style.transform = `translateZ(-${30 * (1 - nextProgress)}px) translateY(${15 * (1 - nextProgress)}px) scale(${0.92 + 0.08 * nextProgress})`;
      nextCover.style.opacity = `${0.75 + 0.25 * nextProgress}`;
    }
  }
}

async function handleMouseUp(e, dotNetRef) {
  if (!isSwiping) {
    resetSwipe(e);
    return;
  }

  touchEndX = e.clientX;
  const coverStack = document.querySelector('.cover-stack');
  const currentCover = e.currentTarget;
  const deltaX = touchEndX - touchStartX;
  const threshold = coverStack.offsetWidth * 0.15; // 降低鼠标操作阈值到15%

  if (Math.abs(deltaX) > threshold) {
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    currentCover.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    currentCover.style.transform = `translateX(${deltaX > 0 ? '120%' : '-120%'}) scale(0.7)`;
    currentCover.style.opacity = '0';
    
    try {
      if (direction === 'next') {
        await dotNetRef.invokeMethodAsync('OnSwipeNext');
      } else {
        await dotNetRef.invokeMethodAsync('OnSwipePrevious');
      }
    } catch (error) {
      console.error('Failed to invoke swipe handler:', error);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
      resetSwipe({ currentTarget: currentCover });
    }, 100);
  } else {
    currentCover.style.transition = 'transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.3s ease';
    resetSwipe(e);
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover) {
      nextCover.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
      setTimeout(() => {
        nextCover.style.transition = '';
      }, 300);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
    }, 300);
  }
  
  isSwiping = false;
}

function resetSwipe(e) {
  const currentCover = e.currentTarget || document.querySelector('.cover-stack-item.current');
  if (currentCover) {
    currentCover.style.transform = '';
    currentCover.style.opacity = '';
  }
  
  const coverStack = document.querySelector('.cover-stack');
  if (coverStack) {
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover) {
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
    }
  }
}
