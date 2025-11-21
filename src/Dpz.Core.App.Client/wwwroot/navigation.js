// 返回按钮处理 - 浏览器历史导航
window.navigationHelper = {
    // 检查是否可以后退
    canGoBack: function () {
        return window.history.length > 1;
    },
    
    // 后退到上一页
    goBack: function () {
        if (window.history.length > 1) {
            window.history.back();
            return true;
        }
        return false;
    },
    
    // 获取当前历史记录长度
    getHistoryLength: function () {
        return window.history.length;
    }
};
