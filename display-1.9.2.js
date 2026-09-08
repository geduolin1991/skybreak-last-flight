/* Reserve real space for HUD and portrait controls. CSS, framebuffer and camera
   share one aspect; resizing happens only after the viewport settles. */
window.createSkybreakDisplay = function ({canvas,stage,mobile,onLayout}) {
 const profiles={balanced:{scale:1.5,pixels:850000},clear:{scale:2,pixels:1150000},smooth:{scale:1,pixels:500000}};
 let quality='balanced';
 try{const saved=localStorage.getItem('skybreak-quality');if(Object.hasOwn(profiles,saved))quality=saved;}catch{}
 let timer=0,resizes=0,lastLayout='',battle=false;
 function apply(){
  if(!mobile)return;
  const box=stage.getBoundingClientRect();if(box.width<1||box.height<1)return;
  const portrait=box.height>box.width;
  stage.classList.toggle('portrait-layout',portrait);stage.classList.toggle('battle-layout',battle);
  const hud=battle?(portrait?88:50):0,deck=battle&&portrait?Math.min(184,box.height*.32):0;
  const available=Math.max(80,box.height-hud-deck);
  const aspect=battle?(portrait?3/4:16/9):portrait?box.width/box.height:16/9;
  const height=Math.min(available,box.width/aspect),width=height*aspect;
  const x=(box.width-width)/2,y=battle?hud:(box.height-height)/2;
  const layout=[width,height,x,y,box.width,box.height].map(n=>n.toFixed(3)).join(':');
  if(layout!==lastLayout){
   lastLayout=layout;canvas.style.width=width+'px';canvas.style.height=height+'px';canvas.style.left=x+'px';canvas.style.top=y+'px';
   for(const [key,value] of Object.entries({'game-width':width,'game-height':height,'game-inset-x':x,'game-inset-y':y,'game-bottom':y+height,'hud-height':hud}))stage.style.setProperty('--'+key,value+'px');
  }
  const profile=profiles[quality],scale=Math.min(window.devicePixelRatio||1,profile.scale,Math.sqrt(profile.pixels/(width*height)));
  let rw,rh;
  if(battle||!portrait){const a=portrait?3:16,b=portrait?4:9,units=Math.max(1,Math.floor(width*scale/a));rw=units*a;rh=units*b;}
  else{rw=Math.max(2,Math.floor(width*scale/2)*2);rh=Math.max(2,Math.round(rw/aspect/2)*2);}
  if(canvas.width!==rw||canvas.height!==rh){canvas.width=rw;canvas.height=rh;resizes++;}
  window.skybreakDisplayState={quality,resizes,width:rw,height:rh,cssWidth:width,cssHeight:height,portrait,battle,hud,deck};
  requestAnimationFrame(()=>onLayout?.());
 }
 function schedule(){clearTimeout(timer);timer=setTimeout(apply,160);}
 if(mobile){apply();new ResizeObserver(schedule).observe(stage);window.addEventListener('orientationchange',schedule);window.addEventListener('resize',schedule);document.addEventListener('fullscreenchange',schedule);}
 return {
  get quality(){return quality;},
  unityConfig:mobile?{matchWebGLToCanvasSize:false,devicePixelRatio:1}:{devicePixelRatio:Math.min(window.devicePixelRatio||1,1.25)},
  setQuality(next){if(!Object.hasOwn(profiles,next)||next===quality)return;quality=next;try{localStorage.setItem('skybreak-quality',quality);}catch{}apply();},
  setBattle(next){if(battle===next)return;battle=next;apply();},
  refresh:schedule
 };
};
