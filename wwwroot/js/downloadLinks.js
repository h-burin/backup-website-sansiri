$(document).ready(function () {
  // ตั้งค่า datatable
  var table = $("#datatable").DataTable();
  table.page.len(25).draw();

  // ✅ อัปเดตสถานะ (เปิด/ปิด Switch)
  $(document).on("change", ".switch-status", function () {
    var urlId = $(this).data("url-id"); // ดึงค่า URL ID
    var isActive = $(this).prop("checked"); // ดึงสถานะ true/false

    // ส่งค่าไปที่ Controller
    $.ajax({
      url: "/BackupWebsite/UpdateStatus", // ✅ ใช้ Path ตรง เพื่อให้ใช้ได้ทุกหน้า
      type: "POST",
      data: { url_id: urlId, is_active: isActive },
      success: function (response) {
        if (response.success) {
          toastr.success("อัปเดตสถานะสำเร็จ!");
        } else {
          toastr.error("อัปเดตไม่สำเร็จ! โปรดลองอีกครั้ง");
        }
      },
      error: function () {
        toastr.error("เกิดข้อผิดพลาดในการอัปเดต!");
      },
    });
  });

  var deleteUrlId = null;

  // ✅ เปิด Modal ยืนยันการลบ
  $(document).on("click", ".delete-btn", function () {
    deleteUrlId = $(this).data("url-id"); // เก็บค่า URL ID
    $("#deleteConfirmText").text(
      "Are you sure you want to delete this item? ID: " + deleteUrlId
    );
    $("#deleteConfirmModal").modal("show");
  });

  // ✅ เมื่อกดปุ่ม "ลบ" ใน Modal
  $("#confirmDeleteBtn").on("click", function () {
    if (deleteUrlId) {
      $.ajax({
        url: "/BackupWebsite/DeleteUrl", // ✅ ใช้ Path ตรง เพื่อให้ใช้ได้ทุกหน้า
        type: "POST",
        data: { url_id: deleteUrlId },
        success: function (response) {
          if (response.success) {
            toastr.success("ลบข้อมูลเรียบร้อยแล้ว!");
            $(`.delete-btn[data-url-id='${deleteUrlId}']`)
              .parents("tr")
              .remove(); // ลบแถวออกจากตาราง
          } else {
            toastr.error("ไม่สามารถลบข้อมูลได้ โปรดลองอีกครั้ง");
          }
        },
        error: function () {
          toastr.error("เกิดข้อผิดพลาดในการลบข้อมูล!");
        },
      });

      $("#deleteConfirmModal").modal("hide"); // ปิด Modal
    }
  });

  // ✅ กดปุ่ม Edit โหลดข้อมูลเข้า Modal
  $(document).on("click", ".edit-btn", function () {
    var urlId = $(this).data("url-id");
    var url = $(this).data("url");
    var thankYouUrl = $(this).data("thankyou-url");
    var categoryId = $(this).data("category-id");

    $("#editUrlId").val(urlId);
    $("#editUrl").val(url);
    $("#editThankYouUrl").val(thankYouUrl);

    // ✅ โหลด Categories และตั้งค่าค่า Default
    loadCategories(function () {
      $("#editCategory").val(categoryId);
    });

    $("#editModal").modal("show");
  });

  // ✅ กดปุ่ม Save เพื่ออัปเดตข้อมูล
  $("#saveEditBtn").on("click", function () {
    var requestData = {
      url_id: parseInt($("#editUrlId").val()),
      url: $("#editUrl").val(),
      url_thankyou: $("#editThankYouUrl").val(),
      id_category_url: parseInt($("#editCategory").val()),
      is_active: true,
      is_delete: false,
    };

    console.log("📌 Debug - ส่งข้อมูลไป API:", requestData); // ✅ ตรวจสอบค่า

    $.ajax({
      url: "/BackupWebsite/UpdateUrl",
      type: "POST",
      contentType: "application/json",
      data: JSON.stringify(requestData),
      success: function (response) {
        if (response.success) {
          toastr.success("Updated successfully!");
          $("#editModal").modal("hide");
          location.reload();
        } else {
          toastr.error("Update failed: " + response.error);
        }
      },
      error: function (xhr) {
        console.log("❌ Debug - API Error:", xhr.responseText);
        toastr.error("Error updating data.");
      },
    });
  });

  // ✅ ให้ DataTable อัปเดต Event Listener โดยไม่ต้อง Bind ใหม่ซ้ำซ้อน
  table.on("draw", function () {
    console.log("Table Redrawn - Events Still Active");
  });
});

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

        if (callback) callback(); // ✅ เรียก callback หลังจากโหลดเสร็จ
      }
    },
    error: function () {
      toastr.error("Failed to load categories.");
    },
  });
}
