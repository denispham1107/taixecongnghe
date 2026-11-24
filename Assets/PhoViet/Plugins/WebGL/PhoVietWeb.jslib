mergeInto(LibraryManager.library,{
 PhoVietSetPixelCap:function(cap){if(window.phoVietSetPixelCap)window.phoVietSetPixelCap(cap);},
 PhoVietSetPixelPlan:function(cap,budget){if(window.phoVietSetPixelPlan)window.phoVietSetPixelPlan(cap,budget);},
 PhoVietTouchDevice:function(){return window.phoVietTouchDevice?1:0;},
 PhoVietSetFootMode:function(enabled){if(window.phoVietSetFootMode)window.phoVietSetFootMode(!!enabled);},
 GotaRunStarted:function(){if(window.gotaRunStarted)window.gotaRunStarted();},
 GotaRunEnded:function(metres,best){if(window.gotaRunEnded)window.gotaRunEnded(metres,best);},
 GotaOpenRanks:function(){if(window.gotaOpenRanks)window.gotaOpenRanks();}
});
