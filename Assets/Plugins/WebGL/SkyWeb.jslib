mergeInto(LibraryManager.library, {
  SkyWebReport: function (json) {
    window.skybreakStatus = JSON.parse(UTF8ToString(json));
  }
});
