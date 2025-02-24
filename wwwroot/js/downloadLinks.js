$(document).ready(function () {
  // ✅ ตั้งค่า DataTable
  var table = $("#datatable").DataTable();
  table.page.len(25).draw();

  // ✅ ฟังก์ชันเปลี่ยนสถานะ (เปิด/ปิด Switch)
  $(document).on("change", ".switch-status", function () {
    var urlId = $(this).data("url-id");
    var isActive = $(this).prop("checked");

    $.ajax({
      url: "/BackupWebsite/UpdateStatus",
      type: "POST",
      data: { url_id: urlId, is_active: isActive },
      success: function (response) {
        if (response.success) {
          toastr.success("Status updated successfully!");
        } else {
          toastr.error("Update failed! Please try again.");
        }
      },
      error: function () {
        toastr.error("An error occurred while updating!");
      },
    });
  });

  var deleteUrlId = null;

  // ✅ เปิด Modal ยืนยันการลบ
  $(document).on("click", ".delete-btn", function () {
    deleteUrlId = $(this).data("url-id");
    $("#deleteConfirmText").text(
      "Are you sure you want to delete this item? ID: " + deleteUrlId
    );
    $("#deleteConfirmModal").modal("show");
  });

  // ✅ ยืนยันการลบข้อมูล
  $("#confirmDeleteBtn").on("click", function () {
    if (deleteUrlId) {
      $.ajax({
        url: "/BackupWebsite/DeleteUrl",
        type: "POST",
        data: { url_id: deleteUrlId },
        success: function (response) {
          if (response.success) {
            toastr.success("Data deleted successfully!");
            $(`.delete-btn[data-url-id='${deleteUrlId}']`)
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

  // ✅ เปิด Modal สำหรับ "Edit" และโหลดข้อมูล
  $(document).on("click", ".edit-btn", function () {
    var urlId = $(this).data("url-id");
    var url = $(this).data("url");
    var thankYouUrl = $(this).data("thankyou-url");
    var categoryId = $(this).data("category-id");

    $("#editUrlId").val(urlId);
    $("#editUrl").val(url);
    $("#editThankYouUrl").val(thankYouUrl);

    // ✅ โหลด Categories ก่อนเปิด Modal
    loadCategories(function () {
      $("#editCategory").val(categoryId);
      $("#editModal").modal("show");
    });

    // ✅ ป้องกัน Event ซ้อนกัน
    $("#saveEditBtn");
    $("#saveEditBtn")
      .off("click")
      .on("click", function () {
        let isEdit = $("#editUrlId").val().trim() !== ""; // ✅ ถ้ามี `url_id` แสดงว่าเป็น Edit
        saveUrl(isEdit);
      });
  });

  // ✅ เปิด Modal สำหรับ "Add" และโหลด Categories
  $("#addNewLinkBtn").on("click", function () {
    $("#editUrlId").val("");
    $("#editUrl").val("");
    $("#editThankYouUrl").val("");

    loadCategories(function () {
      $("#editCategory").val("");
      $("#editModal").modal("show");
    });

    $("#editModalLabel").text("Add Download Link");

    // ✅ ป้องกัน Event ซ้อนกัน
    $("#saveEditBtn")
      .off("click")
      .on("click", function () {
        saveUrl(false);
      });
  });

  // ✅ ฟังก์ชันสำหรับบันทึกข้อมูล (ใช้ได้ทั้ง Add และ Edit)
  function saveUrl(isEdit) {
    if (!validateForm()) {
      toastr.error("Please fill in all required fields.");
      return;
    }

    var urlId = $("#editUrlId").val().trim();
    var url = $("#editUrl").val().trim();
    var thankYouUrl = $("#editThankYouUrl").val().trim();
    var categoryId = $("#editCategory").val();

    var requestData = {
      url: url,
      url_thankyou: thankYouUrl || "",
      id_category_url: categoryId,
    };

    if (isEdit && urlId) {
      requestData.url_id = urlId; // ✅ ส่ง `url_id` เฉพาะตอนแก้ไข
    }

    var apiUrl = isEdit
      ? "/BackupWebsite/UpdateUrl"
      : "/BackupWebsite/AddNewLink";

    if (isEdit && urlId) {
      requestData.url_id = urlId;
    }

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

  // ✅ ฟังก์ชันโหลด Categories
  function loadCategories(callback) {
    $.ajax({
      url: "/BackupWebsite/GetCategories",
      type: "GET",
      success: function (response) {
        if (response.success) {
          var categorySelect = $("#editCategory");
          categorySelect.empty();
          categorySelect.append('<option value="">Select Category</option>');

          response.data.forEach((category) => {
            categorySelect.append(
              `<option value="${category.id_category_url}">${category.name}</option>`
            );
          });

          if (callback) callback();
        }
      },
      error: function () {
        toastr.error("Failed to load categories.");
      },
    });
  }

  // ✅ ให้ DataTable อัปเดต Event Listener โดยไม่ต้อง Bind ใหม่ซ้ำซ้อน
  table.on("draw", function () {
    console.log("Table Redrawn - Events Still Active");
  });
});

// ✅ ฟังก์ชันสำหรับ Validate ข้อมูลก่อนส่ง
function validateForm() {
  let isValid = true;
  let firstInvalid = null;

  if (!$("#editUrl").val().trim()) {
    $("#editUrl").addClass("is-invalid");
    if (!firstInvalid) firstInvalid = $("#editUrl");
    isValid = false;
  } else {
    $("#editUrl").removeClass("is-invalid");
  }

  if (!$("#editCategory").val()) {
    $("#editCategory").addClass("is-invalid");
    if (!firstInvalid) firstInvalid = $("#editCategory");
    isValid = false;
  } else {
    $("#editCategory").removeClass("is-invalid");
  }

  if (firstInvalid) {
    firstInvalid.focus();
  }

  return isValid;
}
