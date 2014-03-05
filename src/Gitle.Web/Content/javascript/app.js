$.ajaxUploadSettings.name = 'uploads';

$.fn.extend({
  insert: function (text) {
    return this.each(function () {
      var textarea = $(this);
      if (textarea[0].nodeName == "TEXTAREA") {
        textarea.val(textarea.val().substr(0, textarea.prop("selectionStart")) + text + textarea.val().substr(textarea.prop("selectionEnd")));
      }
    });
  },
  error: function (error) {
    return this.each(function () {
      $(this).after('<small class="error">' + error + '</small>').parent().addClass("error");
    });
  },
  upload: function () {
    return this.each(function () {
      var textarea = $(this);
      var progressbar = $('<div class="progress">').append($('<span class="meter">')).insertAfter(this).hide();
      if (!$.browser.msie)
        textarea.after($('<small class="info">Je kunt ook afbeeldingen uploaden door ze op het textveld te slepen.</small>'));
      var uploadButton = $('<a href="#" class="button no-margin tiny">Upload afbeelding</a>').insertBefore(this);
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
            textarea.insert('![alt](' + data.Uploads[upload] + ')');
          }
          for (var error in data.Errors) {
            console.log('Error: ' + error + ' - ' + data.Errors[error]);
            textarea.error(error + ': ' + data.Errors[error]);
          }
          progressbar.hide();
        }
      }
      $(this).ajaxUploadDrop(uploadOptions);
      uploadButton.ajaxUploadPrompt(uploadOptions);
    });
  }
});

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
      var newRowNumber = parseInt(table.find('tbody tr:last-child input:first-child').prop('name').match(/\d+/)[0]) + 1;
      var firstRow = table.find('tbody tr:first-child'); // use first row to clone
      var newRow = firstRow.clone();
      // rename all inputs
      newRow.find(':input').each(function () {
        var input = $(this);
        var name = input.prop('name');
        var newName = name.replace(/\d+/g, newRowNumber);
        input.attr('name', newName);
      });

      newRow.find('[type=text]').val(''); // set all values to nothing
      newRow.find('[type=checkbox]').removeProp('checked'); // uncheck checkbox
      newRow.find('.button.alert').click(deleteRow); // set delete action on delete button

      table.find('tbody').append(newRow); // append new row
      config.afterAdd(newRow); // trigger the afterAdd callback from config
    });
    table.after(addRow);
  });
};

var growl = function(type, text) {
  var alert = $('<div>').addClass("alert-box");
  $("body").prepend(alert.addClass(type).html(text));
  alert.delay(2000).fadeOut(2000, function() {
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

function Application() {}
Application.prototype = {
  init: function () {
    var self = this;

    $(document.body).addClass("js-enabled");

    $(window).resize(function () {
      self.windowResize();
    }).resize();

    $('form .focus[value=]').focus();

    $("table.row-clickable tr").click(function () {
      var href = $(this).find("a").first().attr("href");
      if (href) {
        window.location = href;
      }
    });

    $('#initiate-joyride').click(function () {
      $(document).foundation('joyride', 'start');
    });

    $('.tablesorter').tablesorter({});

    self.initMilestoneSelect();
    self.initFreckleSelect();
    self.initCtrlS();

    slugify('#item_Name', '#item_Slug');

    $('.uploadarea').upload();

    $('.colorpicker').colorPicker();
    $('table.nested').nestedTable({
      afterAdd: function (row) {
        row.find('.colorpicker-open').prop('rel', row.find('.colorpicker').prop('name'));
        row.find('.colorpicker').colorPicker();
      }
    });

    marked.setOptions({
      breaks: true
    });
    $('.marked').each(function () {
      $(this).html(marked($(this).html()));
    });

  },

  windowResize: function () {
    if ($(window).height() < $(document).height()) {
      $(document.body).addClass("scroll");
    } else {
      $(document.body).removeClass("scroll");
    }
  },

  initOnLoad: function () {

  },

  initCtrlS: function () {
    $(document).keydown(function (e) {
      if (!(e.which == 83 && e.ctrlKey)) return true;
      $("form").submit();
      e.preventDefault();
      return false;
    });
  },

  initFreckleSelect: function () {
    $('#item_FreckleId').change(function () {
      if ($(this).val() != '0') {
        $('#item_FreckleName').val($(this).find('option:selected').html());
      } else {
        $('#item_FreckleName').val('');
      }
    }).change();
  },

  initMilestoneSelect: function () {
    $('.milestone-new').hide();
    $("#item_Repository").change(function () {
      var fullrepo = $(this).val();
      var url = '/milestone/select?fullrepo=' + fullrepo + '&projectSlug=' + $('#item_Slug').val();
      $('.milestone-select').load(url, function () {
        $('#item_MilestoneId').trigger('change');
      });
    }).change();
    $('form').on('change', '#item_MilestoneId', function () {
      var selected = $(this).find('option:selected');
      if ($(this).val() == 0) {
        $('.milestone-new').show();
        $('#item_MilestoneName').val('');
      } else {
        $('#item_MilestoneName').val(selected.html());
        $('.milestone-new').hide();
      }
    });
  }
};

var app = null;
$(document).ready(function() {
  app = new Application();
  app.init();
});

$(window).load(function() {
  app.initOnLoad();
});