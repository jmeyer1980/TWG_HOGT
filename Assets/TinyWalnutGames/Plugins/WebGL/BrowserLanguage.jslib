mergeInto(LibraryManager.library, {
  GetBrowserLanguage: function() {
    var lang = navigator.language || "en";
    if (typeof UTF8ToString === 'function' && typeof stringToUTF8 === 'function') {
      var bufferSize = lengthBytesUTF8(lang) + 1;
      var buffer = _malloc(bufferSize); // Use globally defined _malloc if still exposed
      if (!buffer) return 0; // Gracefully handle allocation failure
      stringToUTF8(lang, buffer, bufferSize);
      return buffer;
    } else {
      return 0; // Unity's runtime doesn't support expected functions
    }
  }
});