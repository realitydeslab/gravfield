#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (https://www.swig.org).
// Version 4.2.1
//
// Do not make changes to this file unless you know what you are doing - modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class AkChannelEmitter : global::System.IDisposable {
  private global::System.IntPtr swigCPtr;
  protected bool swigCMemOwn;

  internal AkChannelEmitter(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = cPtr;
  }

  internal static global::System.IntPtr getCPtr(AkChannelEmitter obj) {
    return (obj == null) ? global::System.IntPtr.Zero : obj.swigCPtr;
  }

  internal virtual void setCPtr(global::System.IntPtr cPtr) {
    Dispose();
    swigCPtr = cPtr;
  }

  ~AkChannelEmitter() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AkUnitySoundEnginePINVOKE.CSharp_delete_AkChannelEmitter(swigCPtr);
        }
        swigCPtr = global::System.IntPtr.Zero;
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public AkWorldTransform position { set { AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_position_set(swigCPtr, AkWorldTransform.getCPtr(value)); } 
    get {
      global::System.IntPtr cPtr = AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_position_get(swigCPtr);
      AkWorldTransform ret = (cPtr == global::System.IntPtr.Zero) ? null : new AkWorldTransform(cPtr, false);
      return ret;
    } 
  }

  public uint uInputChannels { set { AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_set(swigCPtr, value); }  get { return AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_get(swigCPtr); } 
  }

  public string padding { set { AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_padding_set(swigCPtr, value); }  get { return AkUnitySoundEngine.StringFromIntPtrString(AkUnitySoundEnginePINVOKE.CSharp_AkChannelEmitter_padding_get(swigCPtr)); } 
  }

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.