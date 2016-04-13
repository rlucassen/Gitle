function FoundationHelper() { }
FoundationHelper.prototype = {

  aliases: 'small medium large'.split(/\s/g), //or just Object.keys(Foundation.media_queries)
  events: 'load resize orientationchange',

  getCurrentSizeClass: function () {
    var self = this;
    var matched, unmatched;
    matched = [];
    unmatched = [];

    $.each(self.aliases, function (i, alias) {
      if (window.matchMedia(Foundation.media_queries[alias]).matches) {
        matched.push(alias);
      } else {
        unmatched.push(alias);
      }
    });
    return matched.pop();
  },

  oldSizeClass: undefined,

  registerSizeClassChangeListener: function (onSizeClassChange) {
    var self = this;

    self.onSizeClassChange = onSizeClassChange;

    $(window).on(self.events, function () {
      var newSizeClass = self.getCurrentSizeClass();
      if (newSizeClass != self.oldSizeClass && self.oldSizeClass != undefined) {
        self.onSizeClassChange(newSizeClass);
      }
      self.oldSizeClass = self.getCurrentSizeClass();
    });
  },

  registerSizeClassKnownListener: function (onSizeClassKnown) {
    var self = this;

    self.onSizeClassKnown = onSizeClassKnown;

    $(window).on(self.events, function () {
      var newSizeClass = self.getCurrentSizeClass();
      if (!self.sizeClassKnown) {
        self.onSizeClassKnown(newSizeClass);
      }
      self.sizeClassKnown = true;
    });
  },

  sizeClassKnown: false,
  onSizeClassKnown: undefined, // wat te doen als we er voor het eerst achter komen welke size class we zijn
  onSizeclassChange: undefined // wat te doen bij size class change
};
