$(document).ready(function () {
  // console.log("jQuery Version:", $.fn.jquery);
  // console.log("jQuery Validate Loaded:", typeof $.fn.validate);

  var table = $("#datatable").DataTable();
  table.page.len(25).draw();

  $(".edit-btn").on("click", function () {
    var editCategoryId = null;
    var editCategoryName = $(this).data("name");
    editCategoryId = $(this).data("category-id");

    $("#editCategoryName").val(editCategoryName);
    $("#editCategoryId").val(editCategoryId);

    $("#editModal").modal("show");

    $("#saveEditBtn")
      .off("click")
      .on("click", function () {
        if ($("#editForm").valid()) {
          console.log(editCategoryId);
          if (editCategoryId == undefined || editCategoryId == null) {
            saveCategory(false);
            editCategoryId = null;
          } else {
            saveCategory(true);
            editCategoryId = null;
          }
        }
      });
  });

  function saveCategory(isEdit) {
    var categoryName = $("#editCategoryName").val().trim();
    var categoryId = $("#editCategoryId").val();

    var requestData;

    if (isEdit) {
      var requestData = {
        id_category_url: categoryId,
        name: categoryName,
      };
    } else {
      var requestData = {
        name: categoryName,
      };
    }

    console.log(requestData);

    var apiUrl = isEdit
      ? "/BackupWebsite/UpdateCategory"
      : "/BackupWebsite/AddCategory";

    console.log(apiUrl);
    $.ajax({
      url: apiUrl,
      type: "POST",
      contentType: "application/json",
      data: JSON.stringify(requestData),
      success: function (response) {
        if (response.success) {
          toastr.success(
            isEdit ? "Updated successfully!" : "Added successfully!"
          );
          $("#editModal").modal("hide");
          location.reload();
        } else {
          toastr.error("Error: " + response.error);
        }
      },
      error: function (xhr) {
        toastr.error("Error updating data.");
      },
    });
  }

  var deleteCategoryId = null;

  $(document).on("click", ".delete-btn", function () {
    deleteCategoryId = $(this).data("category-id");
    let deleteName = $(this).data("name");

    $("#deleteConfirmText").html(
      `Are you sure you want to delete this item? Category: <b style="font-weight: 800;">${deleteName}</b>`
    );

    $("#deleteConfirmModal").modal("show");
  });

  // ✅ ยืนยันการลบข้อมูล
  $("#confirmDeleteBtn").on("click", function () {
    if (deleteCategoryId) {
      $.ajax({
        url: "/BackupWebsite/DeleteCategory",
        type: "POST",
        data: { id_category_url: deleteCategoryId },
        success: function (response) {
          if (response.success) {
            toastr.success("Data deleted successfully!");
            $(`.delete-btn[data-category-id='${deleteCategoryId}']`)
              .parents("tr")
              .remove();
          } else {
            toastr.error("Unable to delete data. Please try again.");
          }
        },
        error: function () {
          toastr.error("An error occurred while deleting data!");
        },
      });

      $("#deleteConfirmModal").modal("hide");
    }
  });

  $("#editForm").validate({
    rules: {
      editCategoryName: {
        required: true,
      },
    },
    messages: {
      editCategoryName: {
        required: "กรุณากรอกชื่อหมวดหมู่",
      },
    },
    errorClass: "is-invalid", // ใช้ Bootstrap class
    validClass: "is-valid",
    errorPlacement: function (error, element) {
      element.siblings(".invalid-feedback").text(error.text()).show();
    },
  });

  $("#editModal").on("hidden.bs.modal", function () {
    $(this).find("form")[0].reset(); // รีเซ็ตค่า input
    $(this).find(".is-invalid").removeClass("is-invalid"); // ลบ error class
    $(this).find(".is-valid").removeClass("is-valid"); // ลบ success class
    $(this).find(".invalid-feedback").hide(); // ซ่อนข้อความแจ้งเตือน
  });
});
