$(document).ready(readyFunction);

function readyFunction() {
  $(".competency").on("click", function(event) {
    selectCompetency(event.target);

    event.stopPropagation();
  });

  $(".importanceIndicator").on("click", function(event) {
    $(this).toggleClass("competencyIsImportant");
    $(this).toggleClass("competencyIsNotImportant");
    updateStatus();
  });

  $("#submitButton").on("click", function() {
    saveForm();
  });

  updateStatus();
}

function getRow(childId) {
  return $("#" + childId).parent();
}

function selectCompetency(target) {
  var competencyRow;
  var competencyTd;
  if ($(target).hasClass("competency")) {
    competencyTd = $(target);
    competencyRow = $(target).closest("tr");
  } else {
    competencyTd = $(target).closest(".competency");
    competencyRow = $(competencyTd).closest("tr");
  }

  competencyRow
    .children(".selectedCompetency")
    .removeClass("selectedCompetency");

  competencyTd.addClass("selectedCompetency");

  updateStatus();
}

function updateStatus() {
  var howManyCompetenciesSelected = $(".selectedCompetency").length;
  if (howManyCompetenciesSelected == 0) {
    $("#incompleteFormMessage").text("No competencies selected out of 23.");
    $("#completionWarning").show();
    $("#completionComplete").hide();
  } else if (howManyCompetenciesSelected == 1) {
    $("#incompleteFormMessage").text("One competency selected out of 23.");
    $("#completionWarning").show();
    $("#completionComplete").hide();
  } else if (howManyCompetenciesSelected < 3) {
    $("#incompleteFormMessage").text(
      howManyCompetenciesSelected + " competencies selected out of 23."
    );
    $("#completionWarning").show();
    $("#completionComplete").hide();
  } else {
    $("#completionWarning").hide();
    $("#completionComplete").show();
    $("#completeFormMessage").text("You have completed the form");
  }

  var howManyCompetencies = $(".competencyIsImportant").length;
  if (howManyCompetencies == 0) {
    $(".howManyImportant").text(
      "No competencies have been marked as important to you."
    );
    $("#submitButton").text("Submit with no competencies marked important");
    $(".showImportanceWarning").show();
    $("#thereAreEnoughMarkedImportant").hide();
  } else if (howManyCompetencies == 1) {
    $(".howManyImportant").text("One competency is marked important to you.");
    $(".showImportanceWarning").hide();
    $("#thereAreEnoughMarkedImportant").show();
    $("#submitButton").text("Submit");
  } else {
    $(".howManyImportant").text(
      howManyCompetencies + " competencies are marked as important to you"
    );
    $("#thereAreEnoughMarkedImportant").show();
    $("#submitButton").text("Submit");
    $(".showImportanceWarning").hide();
  }
}

function saveForm() {
  var selectedCompetencies = $(".selectedCompetency")
    .map(function() {
      return this.id;
    })
    .get()
    .join();

  var importantCompetencies = $(".competencyIsImportant")
    .map(function() {
      return this.id;
    })
    .get()
    .join();

  var data = "selected=" + selectedCompetencies;
  data += "&important=" + importantCompetencies;

  var d = new Date();
  var n = d.toLocaleDateString();
  data += "&timestamp=" + n;

  var institutionName = $("#institutionName").val();

  var jsonData = {
    Selected: selectedCompetencies,
    Important: importantCompetencies,
    Date: n,
    Institution: institutionName
  };

  $.ajax({
    url: "https://localhost:5001/api/sat/save",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(jsonData),
    success: function(result) {
      var url = "Self-Assessment-Tool-Wizard/Summary?context=" + result;
      window.location.href = url;
    }
  });

  //   $.post("https://localhost:5001/api/pdf/GenerateSelfAssessmentForm", jsonData)
  //     .then(function(response) {
  //       var url = "Self-Assessment-Tool-Wizard/Summary?" + data;
  //       window.location.href = url;
  //     })
  //     .fail(function(response) {
  //       console.dir(response);
  //     });

  console.log(selectedCompetencies);
  console.log(importantCompetencies);
}
