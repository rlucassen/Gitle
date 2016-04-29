function GitleIssues() { }

GitleIssues.prototype = {
  init: function() {
    this.initFilters();
    this.initThreeStateChecker();
    this.initGroupActions();
    this.initQuickView();
    this.initTimeParser();
  },

  initFilters: function() {
    $('a[data-filter], a[data-filter-clear]').click(function (e) {
      e.preventDefault();
      var filter = $(this).data('filter');
      var oppositeFilter = $(this).data('filter-opposite');
      var query = $('#query').val();
      if ($(this).is('[data-filter-clear]')) query = '';
      if (query.indexOf(oppositeFilter) != -1) {
        query = query.replace(oppositeFilter, "");
      }
      if (oppositeFilter != undefined && oppositeFilter.indexOf(',') !== -1 && oppositeFilter.indexOf(':') === -1) {
        var opposites = oppositeFilter.split(',');
        for (var i in opposites) {
          var rx = new RegExp(opposites[i] + ":[a-zA-Z0-9-_,.]+");
          query = query.replace(rx, '');
        }
      }
      if (query.indexOf(filter) != -1) {
        query = query.replace(filter, "");
      } else {
        query = query + " " + filter;
      }
      $('#query').val(query.replace(/ +(?= )/g, ''));
      $('#query-form').submit();
    });

  },

  initThreeStateChecker: function() {
    $('.three-state-checker').each(function () {
      var checker = $(this);
      checker.append('<i class="fa fa-square-o none"></i><i class="fa fa-check-square-o all"></i><i class="fa fa-minus-square-o some"></i>');
      var selector = $(this).data('selector');
      var checkboxes = $(selector);
      checkboxes.click(function () {
        if (checkboxes.length == checkboxes.filter(':checked').length) {
          checker.attr('data-state', 'all');
        } else if (checkboxes.filter(':checked').length === 0) {
          checker.attr('data-state', 'none');
        } else {
          checker.attr('data-state', 'some');
        }
      });
    }).click(function () {
      var selector = $(this).data('selector');
      var state = $(this).attr('data-state');
      var checkboxes = $(selector);
      switch (state) {
        case "none":
          checkboxes.attr('checked', 'checked');
          $(this).attr('data-state', 'all');
          break;
        default:
          checkboxes.removeAttr('checked');
          $(this).attr('data-state', 'none');
          break;
      }
      checkboxes.change();
    });
  },

  initGroupActions: function() {
    $('#group-actions').hide();
    $('.issues input[type=checkbox][name=issue]').change(function () {
      var checkedBoxes = $('.issues input[type=checkbox][name=issue]:checked');
      if (checkedBoxes.length > 0) {
        $('#group-actions, #exportselection').show();
      } else {
        $('#group-actions, #exportselection').hide();
      }
      var values = [];
      checkedBoxes.each(function () {
        values.push($(this).val());
      });
      $('#group-actions [name=issues]').val(values.join(','));
    }).change();

    $('[data-group-action]').click(function (e) {
      e.preventDefault();
      var issues = $('#group-actions input[name=issues]').val();
      var href = $(this).prop('href');
      location.href = href + (href.indexOf('?') == -1 ? '?' : '&') + 'issues=' + issues;
    });

    $('.issue').click(function () {
      var checkbox = $(this).find('input[type=checkbox]');
      checkbox.prop('checked', !checkbox.is(':checked')).change();
    }).on('click', 'a, input', function (e) {
      e.stopPropagation();
    });
  },

  initQuickView: function() {
    var quickviewDelay = 800;
    $('[data-quickview]').hover(function () {
      var link = $(this);
      if ($(this).data('tooltip') == undefined) {
        var offset = $(this).position();
        var tooltip = $('<div class="quickview">').css('top', offset.top + $(this).height()).css('left', offset.left);
        $(this).data('tooltip', tooltip);
        tooltip.load($(this).data('quickview'), function () {
          tooltip.find('.marked').each(function () {
            $(this).html(marked($(this).html()));
          });
          link.append(tooltip.hide());
          link.data('timeout', setTimeout(function () {
            if (link.is(':hover')) {
              link.data('tooltip').show();
            }
          }, quickviewDelay));
        });
      } else {
        link.data('timeout', setTimeout(function () {
          if (link.is(':hover')) {
            link.data('tooltip').show();
          }
        }, quickviewDelay));
      }
    }, function () {
      $('.quickview').hide();
    });
  },

  initTimeParser: function() {
    $('.time-parser').blur(function () {
      var value = $(this).val();
      if (value === "" || value === "0") return;
      value = value.replace(/[A-Za-z$-]/g, "");
      value = value.replace(",", ".");
      value = parseFloat(value);
      var lessThanQuarter = value < 0.25;
      if (lessThanQuarter) {
        value = 0.25;
      }
      value = value.toString().replace(".", ",");
      $(this).val(value);
      if (lessThanQuarter) {
        $(this).error("minimum is een kwartier");
      }
    });
  }
};

var gitleIssues = null;
$(function () {
  gitleIssues = new GitleIssues();
  gitleIssues.init();
});