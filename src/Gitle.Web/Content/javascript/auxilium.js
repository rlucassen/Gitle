/*
 * Convert value from current elemente into slug and put it in target element [data-slugify]
 */
$.fn.slugify = function () {
  return this.each(function() {
    $(this).change(function () {
      var target = $($(this).data('slugify'));
      $.ajax({
        method: 'POST',
        data: {
          text: $(this).val()
        },
        url: '/slug/index',
        success: function (data) {
          target.val(data);
        }
      });

    }).change();
  });
};

/*
 * Insert text string into textarea at cursor
 */
$.fn.insert = function (text) {
  return this.each(function () {
    var textarea = $(this);
    if (textarea[0].nodeName === "TEXTAREA") {
      textarea.val(textarea.val().substr(0, textarea.prop("selectionStart")) + text + textarea.val().substr(textarea.prop("selectionEnd")));
    }
  });
};

/*
 * Replace all dots into commas
 */
$.fn.commafy = function() {
  return this.replace('.', ',');
};

/*
 * Replace all commas into dots
 */
$.fn.dotify = function() {
  return this.replace(',', '.');
};

$.fn.currency = function() {
  return parseFloat(this).toFixed(2).commafy();
};

/*
 * Show given error below element
 */
$.fn.error = function(error) {
  return this.each(function () {
    $(this).siblings('small.error').remove();
    $(this).after('<small class="error">' + error + '</small>').parent().addClass("error");
  });
}

/*
 * Make given field remember its value by cookies
 */
$.fn.cookieRemember = function() {
  return this.each(function() {
    var name = $(this).data('remember');
    if ($.cookie(name) && self.queryString['query'] == undefined) {
      $(this).val($.cookie(name)).parents('form').submit();
    }
    $.cookie(name, $(this).val());
  }).change(function() {
    var name = $(this).data('remember');
    $.cookie(name, $(this).val());
  });
};

/*
 * Set Ctrl + S to save the forms on the page
 */
$.fn.ctrlS = function() {
  return this.keydown(function(e) {
    if (!(e.which == 83 && e.ctrlKey)) return true;
    $("form").submit();
    e.preventDefault();
    return false;
  });
};

/*
 * Make rows in a table clickable (gets the first link found in the row)
 */
$.fn.tableRowClickable = function() {
  return this.each(function() {
    $(this).find('tr').click(function() {
      var href = $(this).find("a").first().attr("href");
      if (href) {
        window.location = href;
      }
    });
  });
};

/*
 * Make a link ask for confirmation before following link
 */
$.fn.confirm = function() {
  return this.each(function() {
    $(this).click(function() {
      var message = $(this).data('confirm');
      return confirm(message);
    });
  });
};


/*
 * Markdownify the contents of the element
 */
$.fn.markdownify = function() {
  marked.setOptions({
    breaks: true
  });
  return this.each(function() {
    $(this).html(marked($(this).html()));
  });
};

var queryString = [];
var match,
        pl = /\+/g,  // Regex for replacing addition symbol with a space
        search = /([^&=]+)=?([^&]*)/g,
        decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
        query = window.location.search.substring(1);

while (match = search.exec(query))
  queryString[decode(match[1])] = decode(match[2]);
$.ajaxUploadSettings.name = 'uploads';

$.fn.upload = function () {
  return this.each(function () {
    var textarea = $(this);
    var progressbar = $('<div class="progress">').append($('<span class="meter">')).insertAfter(this).hide();
    if (!$.browser.msie)
      textarea.after($('<small class="info">Je kunt ook afbeeldingen (jpg, png, gif) en bestanden (doc, docx, xls, xlsx, pdf, txt) uploaden door ze op het textveld te slepen.</small>'));
    var uploadButton = $('<a href="#" class="button no-margin tiny">Upload afbeelding/bestand</a>').insertBefore(this);
    var uploadOptions = {
      url: '/upload/file',
      beforeSend: function (e, a) {
        progressbar.show();
      },
      onprogress: function (e) {
        if (e.lengthComputable) {
          console.log('Progress ' + e.loaded + ' of ' + e.total);
          var percent = e.loaded / e.total * 100;
          progressbar.find('.meter').css('width', percent + '%');
        }
      },
      error: function () {
        console.log('error');
        textarea.error('Bestand(en) uploaden niet gelukt');
      },
      success: function (data) {
        data = $.parseJSON(data);
        for (var upload in data.Uploads) {
          textarea.insert(data.Uploads[upload]);
        }
        for (var error in data.Errors) {
          console.log('Error: ' + error + ' - ' + data.Errors[error]);
          textarea.error(error + ': ' + data.Errors[error]);
        }
        progressbar.hide();
      }
    };
    $(this).ajaxUploadDrop(uploadOptions);
    uploadButton.ajaxUploadPrompt(uploadOptions);
  });
};

$.fn.uploadList = function() {
  return this.each(function () {
    var list = $(this);
    var templateContainer = $($(this).data('template-container'));
    var template = $($(this).data('template'));
    var url = $(this).data('url');
    var progressbar = $('<div class="progress">').append($('<span class="meter">')).insertAfter(this).hide();
    if (!$.browser.msie)
      list.after($('<small class="info">Je kunt ook afbeeldingen (jpg, png, gif) en bestanden (doc, docx, xls, xlsx, pdf, txt) uploaden door ze op de tabel te slepen.</small>'));
    var uploadButton = $('<a href="#" class="button no-margin tiny">Upload document</a>').insertBefore(this);
    var uploadOptions = {
      url: url,
      beforeSend: function (e, a) {
        progressbar.show();
      },
      onprogress: function (e) {
        if (e.lengthComputable) {
          console.log('Progress ' + e.loaded + ' of ' + e.total);
          var percent = e.loaded / e.total * 100;
          progressbar.find('.meter').css('width', percent + '%');
        }
      },
      error: function () {
        console.log('error');
        list.error('Bestand(en) uploaden niet gelukt');
      },
      success: function (data) {
        data = $.parseJSON(data);
        for (var upload in data.Uploads) {
          var document = data.Uploads[upload];
          var templateHtml = template.html();
          templateHtml = templateHtml.replace(/{{id}}/g, document.Id);
          templateHtml = templateHtml.replace(/{{name}}/g, document.Name);
          templateHtml = templateHtml.replace(/{{path}}/g, document.Path);
          templateHtml = templateHtml.replace(/{{date}}/g, document.DateString);
          templateHtml = templateHtml.replace(/{{uploader}}/g, document.UserFullName);
          templateContainer.append(templateHtml);
        }
        for (var error in data.Errors) {
          console.log('Error: ' + error + ' - ' + data.Errors[error]);
          list.error(error + ': ' + data.Errors[error]);
        }
        progressbar.hide();
      }
    };
    $(this).ajaxUploadDrop(uploadOptions);
    uploadButton.ajaxUploadPrompt(uploadOptions);
  });
};

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