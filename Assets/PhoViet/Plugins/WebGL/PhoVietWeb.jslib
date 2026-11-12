mergeInto(LibraryManager.library,{
 PhoVietSetPixelCap:function(cap){if(window.phoVietSetPixelCap)window.phoVietSetPixelCap(cap);},
 PhoVietTouchDevice:function(){return window.phoVietTouchDevice?1:0;},
 PhoVietSetFootMode:function(enabled){if(window.phoVietSetFootMode)window.phoVietSetFootMode(!!enabled);}
});
