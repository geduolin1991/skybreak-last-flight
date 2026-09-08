/* Keep the camera, drawing buffer and displayed image at the same aspect.
   Resize only after layout settles; browser toolbar animation must not resize
   render targets every frame. Quality changes are explicit, never oscillating. */
window.createSkybreakDisplay = function ({canvas,stage,mobile,onLayout}) {
 const profiles={balanced:{scale:1.5,pixels:850000},clear:{scale:2,pixels:1150000},smooth:{scale:1,pixels:500000}};
 let quality='balanced';
 try {const saved=localStorage.getItem('skybreak-quality');if(Object.hasOwn(profiles,saved))quality=saved;}catch{}
 let timer=0,resizes=0,lastLayout='';
 function apply(){
  if(!mobile)return;
  const box=stage.getBoundingClientRect();if(box.width<1||box.height<1)return;
  const height=Math.min(box.height,box.width*9/16),width=height*16/9;
  const layout=[width,height,box.width,box.height].map(n=>n.toFixed(3)).join(':');
  if(layout!==lastLayout){
   lastLayout=layout;canvas.style.width=width+'px';canvas.style.height=height+'px';
   stage.style.setProperty('--game-width',width+'px');stage.style.setProperty('--game-height',height+'px');
   stage.style.setProperty('--game-inset-x',(box.width-width)/2+'px');stage.style.setProperty('--game-inset-y',(box.height-height)/2+'px');
  }
  const profile=profiles[quality],scale=Math.min(window.devicePixelRatio||1,profile.scale);
  // Integer 16:9 dimensions prevent fractional camera gutters and rounding drift.
  const units=Math.max(1,Math.floor(Math.min(width*scale/16,Math.sqrt(profile.pixels/144))));
  const rw=units*16,rh=units*9;
  if(canvas.width!==rw||canvas.height!==rh){canvas.width=rw;canvas.height=rh;resizes++;}
  window.skybreakDisplayState={quality,resizes,width:rw,height:rh,cssWidth:width,cssHeight:height};
  requestAnimationFrame(()=>onLayout?.());
 }
 function schedule(){clearTimeout(timer);timer=setTimeout(apply,140);}
 if(mobile){
  apply();new ResizeObserver(schedule).observe(stage);
  window.addEventListener('orientationchange',schedule);window.addEventListener('resize',schedule);
  document.addEventListener('fullscreenchange',schedule);
 }
 return {
  get quality(){return quality;},
  unityConfig:mobile?{matchWebGLToCanvasSize:false,devicePixelRatio:1}:{devicePixelRatio:Math.min(window.devicePixelRatio||1,1.25)},
  setQuality(next){if(!Object.hasOwn(profiles,next)||next===quality)return;quality=next;try{localStorage.setItem('skybreak-quality',quality);}catch{}apply();},
  refresh:schedule
 };
};
