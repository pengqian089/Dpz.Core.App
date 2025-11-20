export function scrollToLyric(index) {
  const container = document.getElementById('lyrics-scroll');
  if (!container) return;
  
  const el = document.getElementById(`lyric-${index}`);
  if (!el) return;
  
  // 计算目标位置，让当前歌词居中显示
  const containerHeight = container.clientHeight;
  const elementTop = el.offsetTop;
  const elementHeight = el.clientHeight;
  
  // 目标滚动位置 = 元素顶部 - (容器高度的1/3)，让歌词稍微偏上显示
  const targetScrollTop = elementTop - (containerHeight / 3) + (elementHeight / 2);
  
  container.scrollTo({ 
    top: Math.max(0, targetScrollTop), 
    behavior: 'smooth' 
  });
}

/**
 * 初始化封面手势支持
 * @param {Object} dotNetRef - .NET 对象引用
 */
export function initCoverGesture(dotNetRef) {
  const coverWrapper = document.querySelector('.cover-wrapper');
  if (!coverWrapper) {
    console.warn('Cover wrapper not found');
    return;
  }

  // 移除旧的事件监听器（如果存在）
  coverWrapper.removeEventListener('touchstart', handleTouchStart);
  coverWrapper.removeEventListener('touchmove', handleTouchMove);
  coverWrapper.removeEventListener('touchend', handleTouchEnd);

  // 添加触摸事件
  coverWrapper.addEventListener('touchstart', handleTouchStart, { passive: true });
  coverWrapper.addEventListener('touchmove', handleTouchMove, { passive: false });
  coverWrapper.addEventListener('touchend', (e) => handleTouchEnd(e, dotNetRef), { passive: true });

  // 添加鼠标事件（用于桌面测试）
  coverWrapper.addEventListener('mousedown', handleMouseDown);
  coverWrapper.addEventListener('mousemove', handleMouseMove);
  coverWrapper.addEventListener('mouseup', (e) => handleMouseUp(e, dotNetRef));
  coverWrapper.addEventListener('mouseleave', resetSwipe);
}

/**
 * 清理封面手势事件
 */
export function disposeCoverGesture() {
  const coverWrapper = document.querySelector('.cover-wrapper');
  if (!coverWrapper) return;

  coverWrapper.removeEventListener('touchstart', handleTouchStart);
  coverWrapper.removeEventListener('touchmove', handleTouchMove);
  coverWrapper.removeEventListener('touchend', handleTouchEnd);
  coverWrapper.removeEventListener('mousedown', handleMouseDown);
  coverWrapper.removeEventListener('mousemove', handleMouseMove);
  coverWrapper.removeEventListener('mouseup', handleMouseUp);
  coverWrapper.removeEventListener('mouseleave', resetSwipe);
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
    
    // 如果水平滑动距离大于垂直滑动，认为是滑动操作
    if (deltaX > 10 && deltaX > deltaY) {
      isSwiping = true;
      e.preventDefault(); // 阻止默认滚动
    }
  }
  
  if (isSwiping) {
    e.preventDefault();
    const coverWrapper = e.currentTarget;
    const deltaX = e.touches[0].clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / coverWrapper.offsetWidth));
    
    // 实时更新封面位置和透明度
    coverWrapper.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.1})`;
    coverWrapper.style.opacity = `${1 - Math.abs(progress) * 0.3}`;
  }
}

async function handleTouchEnd(e, dotNetRef) {
  if (!isSwiping) {
    resetSwipe(e);
    return;
  }

  touchEndX = e.changedTouches[0].clientX;
  touchEndY = e.changedTouches[0].clientY;
  
  const coverWrapper = e.currentTarget;
  const deltaX = touchEndX - touchStartX;
  const threshold = coverWrapper.offsetWidth * 0.25; // 25% 的宽度作为阈值

  if (Math.abs(deltaX) > threshold) {
    // 滑动超过阈值，执行切换
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    // 添加滑出动画
    coverWrapper.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    coverWrapper.style.transform = `translateX(${deltaX > 0 ? '100%' : '-100%'}) scale(0.8)`;
    coverWrapper.style.opacity = '0';
    
    // 调用 .NET 方法切换歌曲
    try {
      if (direction === 'next') {
        await dotNetRef.invokeMethodAsync('OnSwipeNext');
      } else {
        await dotNetRef.invokeMethodAsync('OnSwipePrevious');
      }
    } catch (error) {
      console.error('Failed to invoke swipe handler:', error);
    }
    
    // 短暂延迟后重置样式
    setTimeout(() => {
      coverWrapper.style.transition = '';
      resetSwipe({ currentTarget: coverWrapper });
    }, 100);
  } else {
    // 未超过阈值，回弹
    coverWrapper.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    resetSwipe(e);
    setTimeout(() => {
      coverWrapper.style.transition = '';
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
  if (e.buttons !== 1) return; // 只在按下鼠标左键时处理
  
  if (!isSwiping) {
    const deltaX = Math.abs(e.clientX - touchStartX);
    const deltaY = Math.abs(e.clientY - touchStartY);
    
    if (deltaX > 10 && deltaX > deltaY) {
      isSwiping = true;
    }
  }
  
  if (isSwiping) {
    const coverWrapper = e.currentTarget;
    const deltaX = e.clientX - touchStartX;
    const progress = Math.max(-1, Math.min(1, deltaX / coverWrapper.offsetWidth));
    
    coverWrapper.style.transform = `translateX(${deltaX}px) scale(${1 - Math.abs(progress) * 0.1})`;
    coverWrapper.style.opacity = `${1 - Math.abs(progress) * 0.3}`;
  }
}

async function handleMouseUp(e, dotNetRef) {
  if (!isSwiping) {
    resetSwipe(e);
    return;
  }

  touchEndX = e.clientX;
  const coverWrapper = e.currentTarget;
  const deltaX = touchEndX - touchStartX;
  const threshold = coverWrapper.offsetWidth * 0.25;

  if (Math.abs(deltaX) > threshold) {
    const direction = deltaX > 0 ? 'previous' : 'next';
    
    coverWrapper.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    coverWrapper.style.transform = `translateX(${deltaX > 0 ? '100%' : '-100%'}) scale(0.8)`;
    coverWrapper.style.opacity = '0';
    
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
      coverWrapper.style.transition = '';
      resetSwipe({ currentTarget: coverWrapper });
    }, 100);
  } else {
    coverWrapper.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    resetSwipe(e);
    setTimeout(() => {
      coverWrapper.style.transition = '';
    }, 300);
  }
  
  isSwiping = false;
}

function resetSwipe(e) {
  const coverWrapper = e.currentTarget || document.querySelector('.cover-wrapper');
  if (coverWrapper) {
    coverWrapper.style.transform = '';
    coverWrapper.style.opacity = '';
  }
}
