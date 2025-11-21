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

  // 添加触摸事件 - 优化触摸响应
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
let swipeStartTime = 0;

function handleTouchStart(e) {
  touchStartX = e.touches[0].clientX;
  touchStartY = e.touches[0].clientY;
  isSwiping = false;
  swipeStartTime = Date.now();
}

function handleTouchMove(e) {
  if (!isSwiping) {
    const deltaX = Math.abs(e.touches[0].clientX - touchStartX);
    const deltaY = Math.abs(e.touches[0].clientY - touchStartY);
    
    // 降低触发阈值到5px，提高响应性
    if (deltaX > 5 && deltaX > deltaY * 1.5) {
      isSwiping = true;
      e.preventDefault();
    }
  }
  
  if (isSwiping) {
    e.preventDefault();
    const coverStack = document.querySelector('.cover-stack');
    const currentCover = e.currentTarget;
    const deltaX = e.touches[0].clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / (coverStack.offsetWidth * 0.8)));
    
    // 更平滑的变换效果
    currentCover.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.1}) rotateY(${progress * 10}deg)`;
    currentCover.style.opacity = `${1 - Math.abs(progress) * 0.3}`;
    
    // 只在向左滑时显示下一首 - 使用更紧密的参数
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover && deltaX < 0) {
      const nextProgress = Math.min(1, Math.abs(progress));
      nextCover.style.transform = `translateZ(-${15 * (1 - nextProgress)}px) translateY(${8 * (1 - nextProgress)}px) scale(${0.96 + 0.04 * nextProgress})`;
      nextCover.style.opacity = `${0.88 + 0.12 * nextProgress}`;
    } else if (nextCover) {
      // 向右滑时重置下一首的样式
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
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
  const swipeDuration = Date.now() - swipeStartTime;
  const swipeVelocity = Math.abs(deltaX) / swipeDuration; // px/ms
  
  // 根据滑动距离和速度判断是否切换
  // 快速滑动: 速度 > 0.5 px/ms 且滑动 > 30px
  // 慢速滑动: 滑动距离 > 屏幕宽度的15%
  const threshold = coverStack.offsetWidth * 0.15;
  const isQuickSwipe = swipeVelocity > 0.5 && Math.abs(deltaX) > 30;
  const isLongSwipe = Math.abs(deltaX) > threshold;

  if (isQuickSwipe || isLongSwipe) {
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    // 完成滑动动画
    currentCover.style.transition = 'transform 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.35s ease';
    currentCover.style.transform = `translateX(${deltaX > 0 ? '120%' : '-120%'}) scale(0.8) rotateY(${deltaX > 0 ? '20deg' : '-20deg'})`;
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
    // 弹回原位
    currentCover.style.transition = 'transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.35s ease';
    resetSwipe(e);
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover) {
      nextCover.style.transition = 'transform 0.35s ease, opacity 0.35s ease';
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
      setTimeout(() => {
        nextCover.style.transition = '';
      }, 350);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
    }, 350);
  }
  
  isSwiping = false;
}

function handleMouseDown(e) {
  touchStartX = e.clientX;
  touchStartY = e.clientY;
  isSwiping = false;
  swipeStartTime = Date.now();
}

function handleMouseMove(e) {
  if (e.buttons !== 1) return;
  
  if (!isSwiping) {
    const deltaX = Math.abs(e.clientX - touchStartX);
    const deltaY = Math.abs(e.clientY - touchStartY);
    
    // 降低触发阈值到3px
    if (deltaX > 3 && deltaX > deltaY * 1.5) {
      isSwiping = true;
    }
  }
  
  if (isSwiping) {
    const coverStack = document.querySelector('.cover-stack');
    const currentCover = e.currentTarget;
    const deltaX = e.clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / (coverStack.offsetWidth * 0.8)));
    
    // 更平滑的变换效果
    currentCover.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.1}) rotateY(${progress * 10}deg)`;
    currentCover.style.opacity = `${1 - Math.abs(progress) * 0.3}`;
    
    // 只在向左滑时显示下一首 - 使用更紧密的参数
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover && deltaX < 0) {
      const nextProgress = Math.min(1, Math.abs(progress));
      nextCover.style.transform = `translateZ(-${15 * (1 - nextProgress)}px) translateY(${8 * (1 - nextProgress)}px) scale(${0.96 + 0.04 * nextProgress})`;
      nextCover.style.opacity = `${0.88 + 0.12 * nextProgress}`;
    } else if (nextCover) {
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
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
  const swipeDuration = Date.now() - swipeStartTime;
  const swipeVelocity = Math.abs(deltaX) / swipeDuration;
  
  // 鼠标操作阈值稍低
  const threshold = coverStack.offsetWidth * 0.12;
  const isQuickSwipe = swipeVelocity > 0.4 && Math.abs(deltaX) > 25;
  const isLongSwipe = Math.abs(deltaX) > threshold;

  if (isQuickSwipe || isLongSwipe) {
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    // 完成滑动动画
    currentCover.style.transition = 'transform 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.35s ease';
    currentCover.style.transform = `translateX(${deltaX > 0 ? '120%' : '-120%'}) scale(0.8) rotateY(${deltaX > 0 ? '20deg' : '-20deg'})`;
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
    // 弹回原位
    currentCover.style.transition = 'transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.35s ease';
    resetSwipe(e);
    
    const nextCover = coverStack.querySelector('.cover-stack-item.next');
    if (nextCover) {
      nextCover.style.transition = 'transform 0.35s ease, opacity 0.35s ease';
      nextCover.style.transform = '';
      nextCover.style.opacity = '';
      setTimeout(() => {
        nextCover.style.transition = '';
      }, 350);
    }
    
    setTimeout(() => {
      currentCover.style.transition = '';
    }, 350);
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
