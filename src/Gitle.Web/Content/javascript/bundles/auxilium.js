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

var growl = function (type, text) {
  var alert = $('<div>').addClass("alert-box");
  $("body").prepend(alert.addClass(type).html(text));
  alert.delay(2000).fadeOut(2000, function () {
    $(this).remove();
  });
};

var slugify = function (origin, target) {
  $(origin).change(function () {
    $.ajax({
      method: 'POST',
      data: {
        text: $(this).val()
      },
      url: '/slug/index',
      success: function (data) {
        $(target).val(data);
      }
    });

  }).change();
};

$.fn.nestedTable = function (conf) {

  var config = jQuery.extend({
    deleteText: 'Verwijder',
    afterAdd: function (row) { }
  }, conf);

  var deleteRow = function (e) {
    e.preventDefault();
    $(this).parents('tr').remove();
  };

  return this.each(function () {

    var table = $(this);

    var deleteHeader = $('<th>' + config.deleteText + '</th>');
    table.find('thead tr').append(deleteHeader);
    var deleteButton = $('<a href="#" class="button tiny no-margin alert">Verwijder</a>');
    deleteButton.click(deleteRow);
    var deleteCell = $('<td></td>').append(deleteButton);
    table.find('tbody tr').append(deleteCell);

    var addRow = $('<a href="#" class="button small">Nieuw</a>');
    addRow.click(function (e) {
      e.preventDefault();
      var fr = table.find('tbody tr:last-child input:first-child');
      var newRowNumber = parseInt(fr.length == 0 ? 0 : fr.prop('name').match(/\d+/)[0]) + 1;
      var firstRow = table.find('tbody tr:first-child'); // use first row to clone
      var newRow = firstRow.clone().removeClass('hide');
      // rename all inputs
      newRow.find(':input').each(function () {
        var input = $(this);
        var name = input.prop('name');
        var newName = name.replace(/\d+/g, newRowNumber);
        input.attr('name', newName);
      });

      newRow.find('[type=text]').val(''); // set all values to nothing
      newRow.find('[data-nested-remove]').remove();
      newRow.find('[type=checkbox]').removeProp('checked'); // uncheck checkbox
      newRow.find('.button.alert').click(deleteRow); // set delete action on delete button

      table.find('tbody').append(newRow); // append new row
      config.afterAdd(newRow); // trigger the afterAdd callback from config
    });
    table.after(addRow);
  });
};