$.ajaxUploadSettings.name = 'uploads';

$.fn.extend({
  insert: function (text) {
    return this.each(function () {
      var textarea = $(this);
      if (textarea[0].nodeName === "TEXTAREA") {
        textarea.val(textarea.val().substr(0, textarea.prop("selectionStart")) + text + textarea.val().substr(textarea.prop("selectionEnd")));
      }
    });
  },
  error: function (error) {
    return this.each(function () {
      $(this).siblings('small.error').remove();
      $(this).after('<small class="error">' + error + '</small>').parent().addClass("error");
    });
  },
  commafy: function () {
    return this.replace('.', ',');
  },
  dotify: function () {
    return this.replace(',', '.');
  },
  upload: function () {
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
  },
  uploadList: function () {
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
  }
});


function Application() { }
Application.prototype = {
  queryString: {},
  foundationSize: '',
  init: function () {
    var self = this;

    var match,
        pl = /\+/g,  // Regex for replacing addition symbol with a space
        search = /([^&=]+)=?([^&]*)/g,
        decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
        query = window.location.search.substring(1);

    while (match = search.exec(query))
      self.queryString[decode(match[1])] = decode(match[2]);


    $(document.body).addClass("js-enabled");

    var foundationHelper = new FoundationHelper();
    foundationHelper.registerSizeClassChangeListener(onSizeClassChange = function (newSizeClass) {
      self.foundationSize = newSizeClass;
      console.log('changed size class: ' + newSizeClass);
    });

    $(window).resize(function () {
      self.windowResize();
    }).resize();

    $('#search').autocomplete({
      serviceUrl: '/search',
      autoSelectFirst: true,
      onSelect: function (suggestion) {
        window.location = suggestion.data;
      }
    });
    $(document).keydown(function (e) {
      if (e.which == 70 && e.ctrlKey && e.shiftKey) {
        $('#search').focus();
      }
    });

    $('form .focus[value=""]').focus();

    $("table.row-clickable tr").click(function () {
      var href = $(this).find("a").first().attr("href");
      if (href) {
        window.location = href;
      }
    });

    $('#initiate-joyride').click(function () {
      $(document).foundation('joyride', 'start');
    });

    $('.tablesorter').tablesorter({ sortList: [[0, 0]] });

    $(".chosen-select").chosen({ no_results_text: "Oops, nothing found!", width: '100%' });

    $('input[data-remember]').each(function () {
      var name = $(this).data('remember');
      if ($.cookie(name) && self.queryString['query'] == undefined) {
        $(this).val($.cookie(name)).parents('form').submit();
      }
      $.cookie(name, $(this).val());
    }).change(function () {
      var name = $(this).data('remember');
      $.cookie(name, $(this).val());
    });


    self.initFreckleSelect();
    self.initCtrlS();
    self.initComments();

    slugify('#item_Name', '#item_Slug');

    $('.uploadarea').upload();
    $('.uploadlist').uploadList();
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

  initComments: function () {
    $('.comments-container').each(function () {
      var container = $(this);
      var staticComments = container.find('.comments');
      var textarea = container.find('textarea');
      staticComments.click(function () {
        container.addClass('edit');
        textarea.focus();
      });
      textarea.blur(function () {
        $.ajax({
          url: container.data('url'),
          method: 'POST',
          dataType: 'text',
          data: {
            comment: textarea.val()
          },
          success: function (data) {
            staticComments.html(marked(data));
            container.removeClass('edit');
          },
          error: function () {
            console.log('niet opgeslagen');
          }
        });
      });
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
  }
};

var app = null;
$(document).ready(function () {
  app = new Application();
  app.init();
});

$(window).load(function () {
  app.initOnLoad();
});