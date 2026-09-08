mergeInto(LibraryManager.library, {
  SkyWebMobileReport: function (json) {
    window.skybreakMobileState = JSON.parse(UTF8ToString(json));
  },
  SkyWebReport: function (json) {
    window.skybreakStatus = JSON.parse(UTF8ToString(json));
  }
});
