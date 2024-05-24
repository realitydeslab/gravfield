sm=performer("A").pos.plot().tap(sendosc);


a = mousemove(doc)
  .resample(periodic(10))
  .mvavrg({ size: 5 })
  .plot({ legend: 'Mouse Position'})
  .delta({ size: 5 })
  .plot({ legend: 'Mouse velocity' })
  .delta({ size: 5 })
  .plot({ legend: 'Mouse acceleration' })
  .sendosc2("/test");


  // generate a constant unit signal sampled at 1Hz, and accumulate
// the results (sliced at 10 iterations)
const process = periodic(1000)
.constant(1)
.accum()
.tap(log)
.sendosc2("/test")





// 前菜
// 鼠标位置越靠右，线条越粗
const sm1 = mousemove(doc)
.resample(periodic(10))
.mvavrg({ size: 5 })
.select(0)
.mul(now(20))
.plot({ legend: 'Mouse Position'})  
.oscto("ABC", "thickness");

stop(sm1);



// 显示演员的位置
const sm2 = periodic(10)
.performer(0)
.plot()
// 计算演员A和C的位置
.pfm_dist("AC")
.plot()
// 它们越远，线条越粗
.scale({ outmin: 0, outmax: 10 })
.oscto("AC", "thickness");

stop(sm2);


// 所有人的离的越近，震动越灵敏
const sm3 = periodic(10)
.pfm_dist("ABC")
.scale({ inmin:0, inmax:2, outmin: 0, outmax: 100 })
.plot()
.oscto("ABC", "drag");

stop(sm3);


// A的高度 越高，A的线条越沉
const sm4 = periodic(10)
.pfm_maxy("A")
.plot()
.scale({ inmin:0, inmax:3, outmin: 0, outmax: 50 })
.oscto("A", "mass");

stop(sm4);


// 根据节奏改变效果
const sm5 = periodic(1000)
.constant(1)
.osctoall("meshsize")

stop(sm5);

// 辅助Reset
const clearsm = now(3)
.oscto("ABC", "thickness");

clear();