# Hướng dẫn sử dụng và kiểm thử StationeryWarehouse

Tài liệu này hướng dẫn chạy ứng dụng, hiểu luồng nghiệp vụ kho, thao tác từng màn hình và kiểm thử các kết quả chính. Các bước kiểm thử nên được thực hiện trên cơ sở dữ liệu phát triển riêng; không dùng dữ liệu thật hoặc tài khoản mặc định ở môi trường production.

## 1. Tổng quan luồng nghiệp vụ

Ứng dụng quản lý kho sách và văn phòng phẩm theo vị trí Bin. Dữ liệu danh mục và vị trí là nền tảng để lập các chứng từ:

```text
Đăng nhập
  ├─ Danh mục: Sản phẩm, Nhà cung cấp/NXB
  ├─ Kho & vị trí: Kho → Khu vực → Bin
  ├─ Nhập kho: Nháp → Đang nhận → Hoàn tất
  ├─ Xuất kho: Nháp → Đang lấy hàng → Hoàn tất
  ├─ Điều chuyển: Nháp → Hoàn tất
  ├─ Kiểm kê: Nháp → Đang kiểm kê → Hoàn tất
  └─ Tồn kho: Tra cứu kết quả và kiểm tra biến động
```

Chỉ chứng từ hoàn tất mới làm thay đổi tồn kho chính thức. Khi hoàn tất nhập, xuất, điều chuyển hoặc kiểm kê, ứng dụng ghi nhận biến động kho. Phiếu đã hoàn tất không thể dùng để sửa trực tiếp số tồn; khi có chênh lệch cần lập chứng từ nghiệp vụ thích hợp.

## 2. Khởi động ứng dụng

### Yêu cầu

- .NET SDK 10.
- SQL Server đang chạy và có thể truy cập từ máy chạy ứng dụng.
- Cấu hình kết nối database và khóa JWT trong cấu hình cục bộ.

Ứng dụng đọc connection string `ConnectionStrings:DefaultConnection` và phần cấu hình `JwtSettings` gồm `Key`, `Issuer`, `Audience`, `ExpirationMinutes`. Khóa `JwtSettings:Key` phải là giá trị bí mật riêng của môi trường, đủ độ dài theo yêu cầu JWT; không đưa khóa hoặc mật khẩu thật vào Git.

Ví dụ cấu hình dạng biến môi trường (thay các giá trị mẫu bằng cấu hình cục bộ):

```text
ConnectionStrings__DefaultConnection=<connection string SQL Server>
JwtSettings__Key=<khóa bí mật riêng, đủ dài>
JwtSettings__Issuer=StationeryWarehouse
JwtSettings__Audience=StationeryWarehouseWeb
JwtSettings__ExpirationMinutes=30
```

Từ thư mục gốc dự án, chạy:

```bash
dotnet restore
dotnet run
```

Seeder chạy khi ứng dụng khởi động, tự áp dụng migration và bổ sung dữ liệu demo còn thiếu. Mở URL mà dòng `Now listening on:` in ra trong terminal, thường là `http://localhost:xxxx` hoặc `https://localhost:xxxx`. Đăng nhập tại `/Auth/Login`.

Nếu khởi động thất bại:

1. Xác nhận SQL Server đang chạy và connection string trỏ đúng server/database.
2. Xác nhận đã cấu hình `JwtSettings:Key`, `Issuer`, `Audience`.
3. Đọc lỗi đầu tiên trong terminal; lỗi kết nối database thường xuất hiện trước khi trang đăng nhập hoạt động.
4. Không xóa database hoặc migration để xử lý lỗi trước khi sao lưu và xác định nguyên nhân.

## 3. Tài khoản, vai trò và quyền

Seeder tạo các tài khoản mặc định sau nếu chúng chưa tồn tại:

| Tài khoản | Mật khẩu phát triển mặc định | Vai trò |
|---|---|---|
| `admin` | `Admin@123` | Quản trị viên |
| `manager` | `Manager@123` | Quản lý kho |
| `operator` | `Operator@123` | Nhân viên kho |
| `viewer` | `Viewer@123` | Người xem |

Đây là thông tin đăng nhập **chỉ dành cho database phát triển mới, cô lập**. Seeder không ghi đè mật khẩu của tài khoản đã tồn tại. Không sử dụng hoặc giữ các mật khẩu này trên môi trường triển khai thật; hãy đổi mật khẩu và tạo khóa JWT riêng trước khi đưa ứng dụng ra ngoài máy phát triển.

| Vai trò | Dùng để kiểm thử |
|---|---|
| ADMIN | Toàn quyền; truy cập Cài đặt và quản lý người dùng. |
| WAREHOUSE_MANAGER | Thao tác nghiệp vụ kho; có quyền quản lý danh mục nhà cung cấp/NXB. |
| WAREHOUSE_OPERATOR | Thực hiện các nghiệp vụ nhập, xuất, điều chuyển và kiểm kê được cấp quyền. |
| VIEWER | Kiểm tra quyền xem; không được phép thực hiện các thao tác bị giới hạn theo vai trò. |

Các tài khoản `demo.user01` đến `demo.user12` là dữ liệu mẫu người dùng và được tạo ở trạng thái **không hoạt động**, không dùng các tài khoản này để đăng nhập. Để kiểm tra quyền, đăng xuất rồi đăng nhập lại bằng tài khoản có vai trò cần thử. Từ chối truy cập được chuyển tới trang `/Auth/AccessDenied`.

## 4. Dữ liệu demo và cách chọn dữ liệu test

Seeder cung cấp dữ liệu danh mục, kho/vị trí, tồn kho và chứng từ ở nhiều trạng thái. Mã nhận diện phổ biến:

| Loại dữ liệu | Mã tham khảo |
|---|---|
| Kho | `WH01`, `WH02` |
| Bin demo | `DEMO-A-01` … `DEMO-A-12` (kho WH01); `DEMO-B-01` … `DEMO-B-12` (kho WH02) |
| Sách demo | `DEMO-BOOK-001` … `DEMO-BOOK-015` |
| Văn phòng phẩm demo | `DEMO-VPP-001` … `DEMO-VPP-015` |
| Nhà cung cấp demo | `DEMO-SUP-001` … `DEMO-SUP-014` |
| Phiếu nhập demo | `DEMO-IN-2026-001` … `DEMO-IN-2026-012` |
| Phiếu xuất demo | `DEMO-OUT-2026-001` … `DEMO-OUT-2026-012` |
| Phiếu điều chuyển demo | `DEMO-TR-2026-001` … `DEMO-TR-2026-012` |
| Phiếu kiểm kê demo | `DEMO-ST-2026-001` … `DEMO-ST-2026-012` |

Một số bản ghi demo được đặt không hoạt động để thử bộ lọc/trạng thái. Khi lập chứng từ, chọn sản phẩm, nhà cung cấp, kho và Bin **đang hoạt động** từ danh sách/autocomplete. Không giả định tồn kho của một mã luôn bằng số lượng ban đầu: các chứng từ demo trạng thái Hoàn tất đã tạo biến động, và mỗi lần chạy thử nghiệp vụ có thể tiếp tục thay đổi tồn.

Seeder được thiết kế để không tạo lặp các bản ghi demo đã có. Chạy lại ứng dụng không phải là thao tác reset dữ liệu và không hoàn nguyên các phiếu do người dùng tạo hoặc đã hoàn tất.

## 5. Cách dùng danh sách, bộ lọc và phân trang

Các màn danh sách nghiệp vụ hiển thị tối đa **10 dòng mỗi trang**. Tại chân bảng:

- Dùng nút trang hoặc mũi tên để chuyển trang.
- Bộ lọc/từ khóa được giữ trong URL khi chuyển trang.
- Kho và vị trí trên màn Kho & vị trí, hoặc Users/Roles trên Cài đặt, có phân trang độc lập.
- Khi kết quả ít hơn hoặc bằng 10 dòng, có thể không xuất hiện điều hướng sang trang tiếp theo.
- Xóa bộ lọc hoặc quay về trang đầu nếu trang hiện tại không còn dữ liệu sau khi lọc.

Để kiểm thử: mở một danh sách có trên 10 bản ghi, xác nhận trang đầu có tối đa 10 dòng, chuyển sang trang 2, rồi thử tìm kiếm/trạng thái và kiểm tra số dòng/kết quả được cập nhật đúng.

## 6. Hướng dẫn theo chức năng

### 6.1. Đăng nhập và trang chủ

1. Mở `/Auth/Login`, nhập tên đăng nhập và mật khẩu.
2. Sau khi đăng nhập, kiểm tra tên người dùng và vai trò hiển thị trên thanh điều hướng.
3. Mở **Trang chủ** để xem bảng điều khiển/tóm tắt.
4. Đăng xuất bằng thao tác trên thanh người dùng; xác nhận truy cập lại màn chức năng yêu cầu đăng nhập sẽ đưa về trang đăng nhập.

**Kiểm thử:** thử sai mật khẩu; ứng dụng phải từ chối đăng nhập và không tạo phiên đã xác thực. Thử đăng nhập bằng một tài khoản demo không hoạt động; tài khoản phải bị từ chối.

### 6.2. Sản phẩm

Vào **Sản phẩm**. Dùng tìm kiếm/bộ lọc và phân trang để tra cứu; mở chi tiết/chỉnh sửa bản ghi theo các thao tác hiển thị. Tạo sản phẩm mới khi cần dữ liệu riêng cho một kịch bản:

1. Chọn loại **Sách** hoặc **Văn phòng phẩm**.
2. Nhập mã sản phẩm, tên, barcode, đơn vị tính và mức tồn tối thiểu.
3. Điền trường đặc thù nếu có: sách có thông tin như ISBN/tác giả/NXB; văn phòng phẩm có thông tin như thương hiệu/màu/quy cách.
4. Lưu và xác nhận sản phẩm xuất hiện trong danh sách/tìm kiếm.

**Kiểm thử:** lưu sản phẩm có barcode trùng với bản ghi khác; ứng dụng phải báo lỗi validation. Thử tìm sản phẩm bằng mã, tên hoặc barcode. Không hoạt động hóa sản phẩm đang được dùng trong chứng từ thực tế nếu chưa đánh giá ảnh hưởng.

### 6.3. Nhà cung cấp / Nhà xuất bản

Vào **Nhà cung cấp / NXB**. Tài khoản ADMIN hoặc WAREHOUSE_MANAGER có thể tạo, sửa và ngừng hoạt động hóa bản ghi. Nhập mã, tên, loại đối tác (nhà cung cấp/NXB/cả hai) và thông tin liên hệ phù hợp.

**Kiểm thử:** tìm theo mã/tên; kiểm tra trạng thái đang hoạt động; thử tạo mã trùng. Dùng tài khoản operator/viewer để xác nhận thao tác quản lý danh mục bị giới hạn theo quyền. Ngừng hoạt động là cách loại khỏi lựa chọn nghiệp vụ, không phải xóa lịch sử chứng từ.

### 6.4. Kho và vị trí

Vào **Kho & vị trí**:

1. Tạo hoặc mở kho (dữ liệu demo thường có `WH01` và `WH02`).
2. Tạo cấu trúc vị trí thuộc kho: khu vực và các Bin chứa hàng.
3. Đặt mã/barcode nhận diện, loại vị trí và trạng thái hoạt động.
4. Xác nhận Bin thuộc đúng kho; dùng bộ lọc/phân trang để tra cứu.

**Kiểm thử:** tạo một Bin hoạt động mới trong kho test; chọn Bin đó trong autocomplete ở chứng từ. Thử chọn Bin inactive hoặc Bin thuộc kho khác: nghiệp vụ phải từ chối vị trí không phù hợp. Không dùng cùng một vị trí làm nguồn và đích của một điều chuyển.

### 6.5. Tồn kho

Vào **Tồn kho** để xem số lượng theo sản phẩm/Bin. Dùng bộ lọc kho, vị trí, loại sản phẩm, từ khóa và trạng thái tồn thấp nếu có. Mở chi tiết sản phẩm để xem thông tin tồn liên quan.

**Kiểm thử:**

1. Ghi lại số lượng sản phẩm cần thử tại Bin nguồn trước khi lập chứng từ.
2. Dùng bộ lọc kho hoặc loại sản phẩm; xác nhận kết quả chỉ thuộc điều kiện đã chọn.
3. Mở trang chi tiết, sau đó quay lại danh sách và thử phân trang.
4. Sau khi hoàn tất một phiếu nhập/xuất/điều chuyển/kiểm kê, tìm lại đúng sản phẩm và Bin để xác nhận delta tồn.

Tồn thấp được xác định theo mức tồn tối thiểu của sản phẩm; đây là cảnh báo, không phải trạng thái chứng từ.

### 6.6. Nhập kho

Luồng trạng thái:

```text
DRAFT (Nháp) → RECEIVING (Đang nhận) → DONE (Hoàn tất)
        └────────────────────────────→ CANCELLED (Đã hủy)
```

Các bước:

1. Vào **Nhập kho** → tạo phiếu.
2. Chọn nhà cung cấp, kho nhận và ngày; thêm ít nhất một dòng sản phẩm cùng Bin thuộc kho nhận.
3. Nhập số lượng dự kiến và lưu phiếu ở trạng thái Nháp.
4. Mở chi tiết, chọn **Bắt đầu nhận** để chuyển sang Đang nhận.
5. Sửa phiếu để nhập số lượng thực nhận (không vượt số lượng dự kiến); kiểm tra Bin cất hàng trên từng dòng.
6. Chọn **Xác nhận nhập**. Phiếu chuyển sang Hoàn tất, tồn tại Bin nhận tăng theo số lượng thực nhận và phát sinh biến động loại IN.

**Ca kiểm thử:**

- Thành công: dự kiến 5, thực nhận 4, Bin hợp lệ → hoàn tất thành công; tồn tăng 4.
- Validation: thực nhận lớn hơn dự kiến, bằng 0, âm hoặc thiếu Bin → không được hoàn tất; thông báo phải chỉ ra dữ liệu cần sửa.
- Hủy: hủy phiếu Nháp/Đang nhận → trạng thái Đã hủy; không cộng tồn.
- Phiếu đã Hoàn tất không được sửa hoặc hoàn tất lần nữa.

### 6.7. Xuất kho

Luồng trạng thái:

```text
DRAFT (Nháp) → PICKING (Đang lấy hàng) → DONE (Hoàn tất)
        └─────────────────────────────→ CANCELLED (Đã hủy)
```

Các bước:

1. Vào **Xuất kho** → tạo phiếu; chọn kho xuất, lý do xuất và ngày.
2. Thêm sản phẩm, Bin nguồn trong kho xuất, số lượng yêu cầu và số lượng lấy. Chọn Bin có đủ tồn.
3. Lưu phiếu Nháp; kiểm tra lại dòng hàng và số lượng trước khi bắt đầu lấy.
4. Mở chi tiết, chọn **Bắt đầu lấy hàng**.
5. Chọn hoàn tất. Tổng số lượng lấy theo từng sản phẩm phải bằng tổng số lượng yêu cầu; tồn Bin phải đủ.
6. Sau khi hoàn tất, tồn tại Bin nguồn giảm theo số lượng lấy và phát sinh biến động loại OUT.

**Ca kiểm thử:**

- Thành công: lấy đúng toàn bộ lượng yêu cầu trong phạm vi tồn → hoàn tất và tồn giảm đúng lượng.
- Lấy ít hơn yêu cầu hoặc nhiều hơn yêu cầu → không hoàn tất được.
- Chọn Bin thiếu tồn, Bin inactive hoặc Bin khác kho → không hoàn tất được.
- Hủy phiếu trước khi hoàn tất → không trừ tồn.
- Thử hoàn tất lần thứ hai với phiếu DONE → ứng dụng từ chối.

### 6.8. Điều chuyển

Luồng: `DRAFT (Nháp) → DONE (Hoàn tất)` hoặc `DRAFT → CANCELLED`.

1. Vào **Điều chuyển** → tạo phiếu.
2. Chọn kho nguồn và kho đích khác nhau.
3. Thêm sản phẩm, Bin nguồn thuộc kho nguồn, Bin đích thuộc kho đích và số lượng dương.
4. Lưu rồi mở chi tiết; xác nhận hai kho/Bin và sản phẩm.
5. Chọn **Hoàn tất điều chuyển**.

Khi thành công, tồn Bin nguồn giảm và tồn Bin đích tăng cùng một lượng; tổng tồn của sản phẩm trên hai vị trí không đổi. Hệ thống ghi nhận biến động xuất khỏi nguồn và nhập vào đích.

**Ca kiểm thử:** chuyển lượng nhỏ hơn hoặc bằng tồn → hoàn tất; thử chuyển vượt tồn, dùng cùng Bin làm nguồn/đích, chọn Bin sai kho, hoặc chọn cùng kho nguồn/đích → phải bị từ chối. Hủy phiếu Nháp không thay đổi tồn.

### 6.9. Kiểm kê và điều chỉnh

Luồng trạng thái:

```text
DRAFT (Nháp) → COUNTING (Đang kiểm kê) → DONE (Hoàn tất)
        └──────────────────────────────→ CANCELLED (Đã hủy)
```

Thao tác theo giao diện hiện tại:

1. Vào **Kiểm kê** → tạo phiếu, chọn kho.
2. Khi phiếu còn Nháp, thêm sản phẩm và Bin; kiểm tra cột **SL hệ thống** được chụp từ tồn tại Bin.
3. Nhập **SL thực tế** cho từng dòng khi phiếu còn Nháp rồi lưu.
4. Mở chi tiết và chọn **Bắt đầu kiểm kê**.
5. Kiểm tra lại số lượng/chênh lệch rồi chọn **Hoàn tất kiểm kê** để áp dụng điều chỉnh.
6. Chênh lệch được tính bằng `SL thực tế - SL hệ thống`: dương làm tăng tồn, âm làm giảm tồn; biến động được ghi nhận.

**Ca kiểm thử:**

- Không chênh lệch: nhập số thực tế bằng số hệ thống → sau hoàn tất tồn không đổi.
- Thừa hàng: nhập số thực tế lớn hơn hệ thống → sau hoàn tất tồn tăng đúng phần chênh.
- Thiếu hàng: nhập số thực tế nhỏ hơn hệ thống → sau hoàn tất tồn giảm đúng phần chênh.
- Không có dòng sản phẩm hoặc số thực tế âm → không được bắt đầu/hoàn tất.
- Hủy phiếu trước hoàn tất → không điều chỉnh tồn.

> Lưu ý: ở bản hiện tại, form cho nhập số thực tế khi phiếu còn Nháp; sau khi chuyển sang Đang kiểm kê, form khóa các trường cấu hình/đếm. Vì vậy cần nhập và lưu số đếm trước khi bấm **Bắt đầu kiểm kê**.

### 6.10. Cài đặt

Chỉ ADMIN truy cập **Cài đặt**. Màn hình có danh sách người dùng và vai trò:

- Tạo người dùng với tên, thông tin cần thiết và vai trò.
- Sửa người dùng, bật/tắt hoạt động hoặc đổi mật khẩu theo thao tác được cung cấp.
- Chuyển tab Người dùng/Vai trò; mỗi danh sách có phân trang độc lập.
- Không bật hoạt động cho các tài khoản demo không cần dùng.

**Kiểm thử:** tạo tài khoản test với vai trò VIEWER, đăng xuất và đăng nhập bằng tài khoản đó; kiểm tra chỉ xem được chức năng phù hợp. Đăng nhập lại ADMIN để khóa tài khoản test sau khi xong. Kiểm tra tài khoản bị khóa không thể đăng nhập.

## 7. Checklist kiểm thử tổng hợp

Thực hiện trên database test và dùng bản ghi riêng nếu cần giữ lại dữ liệu demo:

- [ ] Khởi động ứng dụng và đăng nhập ADMIN thành công.
- [ ] Đăng nhập sai mật khẩu và tài khoản inactive bị từ chối.
- [ ] Danh sách có hơn 10 bản ghi chỉ hiển thị tối đa 10 dòng/trang; chuyển trang giữ bộ lọc.
- [ ] Tạo một sản phẩm và một nhà cung cấp hợp lệ; kiểm tra tìm kiếm và lỗi mã/barcode trùng.
- [ ] Tạo hoặc chọn kho, Bin hoạt động; xác nhận Bin được lọc đúng theo kho trong form chứng từ.
- [ ] Hoàn tất phiếu nhập: tồn tăng bằng số lượng thực nhận; hủy phiếu không làm tăng tồn.
- [ ] Hoàn tất phiếu xuất: tồn giảm bằng số lượng lấy; vượt tồn hoặc thiếu lượng lấy không được phép.
- [ ] Hoàn tất điều chuyển: tồn nguồn giảm, tồn đích tăng tương ứng; tổng tồn không đổi.
- [ ] Hoàn tất kiểm kê: tồn điều chỉnh đúng theo chênh lệch; hủy không thay đổi tồn.
- [ ] Phiếu DONE không cho sửa/hủy/hoàn tất lặp.
- [ ] ADMIN truy cập Cài đặt; kiểm tra tài khoản vai trò thấp hơn không có quyền quản trị bị giới hạn.
- [ ] Đăng xuất xong, truy cập lại URL yêu cầu đăng nhập sẽ chuyển tới trang đăng nhập.

## 8. Mẹo đối chiếu kết quả tồn

Trước mỗi bài test có thay đổi tồn, ghi lại cặp **sản phẩm + Bin** và lượng hiện tại trên màn Tồn kho:

| Nghiệp vụ | Kỳ vọng sau hoàn tất |
|---|---|
| Nhập | `Tồn mới = Tồn cũ + SL thực nhận` |
| Xuất | `Tồn mới = Tồn cũ - SL lấy` |
| Điều chuyển | `Tồn nguồn mới = Tồn nguồn cũ - SL`; `Tồn đích mới = Tồn đích cũ + SL` |
| Kiểm kê | Chênh lệch được áp dụng vào tồn hiện tại; nếu không có biến động nào khác kể từ lúc chụp SL hệ thống thì `Tồn mới = SL thực tế` |
| Hủy phiếu chưa hoàn tất | Tồn không đổi |

Nếu kết quả không như mong đợi, kiểm tra trước: trạng thái chứng từ có phải DONE không, đúng sản phẩm/Bin chưa, Bin có thuộc đúng kho và đang hoạt động không, số lượng đã được lưu ở form chưa, và có chứng từ khác vừa làm thay đổi tồn không.

## 9. Giới hạn và lưu ý dữ liệu

- Dữ liệu seed demo bổ sung theo mã định danh, không làm sạch hoặc reset dữ liệu hiện có.
- Không xóa dữ liệu trực tiếp trong database để “làm lại” một phiếu đã hoàn tất; tạo phiếu test mới hoặc khôi phục database test từ bản sao lưu.
- Chỉ dùng chứng từ test cho dữ liệu thử nghiệm. Chứng từ DONE là lịch sử nghiệp vụ và tác động đến tồn.
- Khi báo lỗi, ghi lại vai trò đang dùng, URL/màn hình, mã phiếu, sản phẩm, Bin, trạng thái, các bước tái hiện và thông báo hiển thị; không gửi mật khẩu, khóa JWT hoặc connection string.
