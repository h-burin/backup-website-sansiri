$(document).ready(function () {
  var table = $("#datatable").DataTable();
  table.page.len(25).draw();

  var deleteUrlId = null; // ตัวแปรเก็บ URL ID ที่จะลบ

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

  // ✅ เปิด Modal ยืนยันการลบ
  $(document).on("click", ".delete-btn", function () {
    deleteUrlId = $(this).data("url-id"); // เก็บค่า URL ID
    $("#deleteConfirmText").text(
      "Are you sure you want to delete this item? ID: " + deleteUrlId
    );
    $("#deleteConfirmModal").modal("show"); // เปิด Modal
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

  // ✅ ให้ DataTable อัปเดต Event Listener โดยไม่ต้อง Bind ใหม่ซ้ำซ้อน
  table.on("draw", function () {
    console.log("Table Redrawn - Events Still Active");
  });
});
