$(function () {
  $('#project-chooser').autocomplete({
    serviceUrl: '/project/autocomplete',
    autoSelectFirst: true,
    onSelect: function (suggestion) {
      $('#projectId').val(suggestion.data);
      $('#project-chooser').val(suggestion.value);
    }
  });

  $('.date').fdatepicker({
    format: 'dd-mm-yyyy',
    weekStart: 1
  });

});