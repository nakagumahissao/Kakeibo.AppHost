
# Kakeibo --- Personal Finance Manager

![.NET](https://img.shields.io/badge/.NET_9-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_9-5C2D91?logo=dotnet)
![Microsoft SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![CSharp](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)

Kakeibo is a full-stack personal finance management system inspired by the traditional Japanese budgeting method.  
It provides expense tracking, category organization, authentication, and full CRUD capabilities for all finance‑related entities.

---

# 🇺🇸 English Version

## 🚀 Features

### 🔐 Authentication & Security
- User registration & login  
- Secure password hashing  
- Password reset with email token  
- JWT authentication (when implemented)

### 💰 Financial Management
- CRUD for:
  - Expenses  
  - Categories  
  - Payment Methods  
  - Accounts (if added later)
- Filtering, sorting and searching  
- Model validation  

### 🧱 Backend
- ASP.NET Core 9 MVC + REST endpoints  
- Entity Framework Core 9  
- SQL Server database (LocalDB or container)  
- Repository & Service layer patterns  
- Full async/await support  
- Clean separation of concerns  

### 🧩 Frontend (Client)
- ASP.NET MVC Razor Frontend  
- Clean UI for entering, editing, and viewing expenses  
- Client‑side validation  
- REST communication with the API  

---

## 📁 Project Structure

```
/Kakeibo
│
├── Kakeibo.Api/            # REST API (ASP.NET 9)
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Entities/
│   ├── DTO/
│   └── Kakeibo.Api.csproj
│
├── Kakeibo.Web/            # MVC frontend client
│   ├── Controllers/
│   ├── Views/
│   ├── ViewModels/
│   └── Kakeibo.Web.csproj
│
├── Kakeibo.Tests/          # Unit tests (xUnit)
│
└── README.md               # You are here
```

---

## 🛠️ Tech Stack

| Layer    | Technology                |
|----------|---------------------------|
| Frontend | ASP.NET MVC 9 (Razor)     |
| API      | ASP.NET Core Web API 9    |
| Database | SQL Server                |
| ORMs     | EF Core 9                 |
| Auth     | ASP.NET Identity          |
| Language | C# (.NET 9)               |

---

## 📦 Setup Instructions

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USER/kakeibo.git
cd kakeibo
```

### 2. Configure AppSettings
Both *API* and *Web* projects require valid:

- SQL Server connection strings  
- Identity settings  
- Client URLs  
- Email provider credentials (for password reset)

### 3. Apply Migrations
Inside the API project:
```bash
dotnet ef database update --project Kakeibo.Api
```

### 4. Run the Projects
Run both API and Web:
```bash
dotnet run --project Kakeibo.Api
dotnet run --project Kakeibo.Web
```

---

## 🔧 Development Notes

- Password reset uses an encoded identity token passed in the URL:  
  `/resetpassword/{userId}?token={encodedToken}`  
- The generated token **never persists**; it must be stored or sent immediately (email).  
- The API uses DTOs — no direct entity exposure.  

---

## 📘 Future Improvements
- Dashboard with charts  
- Monthly report generation  
- Export to CSV/Excel  
- Budgeting goals  
- Mobile‑friendly UI  
- Import from credit card/OFX files  

---

## 🤝 Contributing
Pull requests are welcome. For major changes, open an issue first.

---

## 📄 License
MIT License — feel free to use, modify, and distribute.

---

## 🙏 Acknowledgements
Inspired by the Japanese household accounting method 家計簿 (*Kakeibo*).

---

# 🇧🇷 Versão em Português (Brasil)

## 🚀 Funcionalidades

### 🔐 Autenticação & Segurança
- Cadastro e login de usuários  
- Hash seguro de senhas  
- Redefinição de senha com token enviado por e-mail  
- Autenticação JWT (quando implementado)

### 💰 Gestão Financeira
- CRUD para:
  - Despesas  
  - Categorias  
  - Métodos de Pagamento  
  - Contas (se adicionadas futuramente)
- Filtragem, ordenação e pesquisa  
- Validação de modelos  

### 🧱 Backend
- ASP.NET Core 9 MVC + endpoints REST  
- Entity Framework Core 9  
- Banco de dados SQL Server (LocalDB ou container)  
- Padrões Repository & Service  
- Suporte completo a async/await  
- Separação limpa de responsabilidades  

### 🧩 Frontend
- ASP.NET MVC Razor  
- Interface limpa para registrar, editar e visualizar despesas  
- Validação no lado do cliente  
- Comunicação REST com a API  

---

# 🇯🇵 日本語版

## 🚀 機能

### 🔐 認証とセキュリティ
- ユーザー登録・ログイン  
- 安全なパスワードハッシュ化  
- メールによるパスワードリセットトークン  
- JWT 認証（実装予定）

### 💰 家計管理
- CRUD 操作:
  - 支出  
  - カテゴリー  
  - 支払方法  
  - アカウント（将来的に追加可能）
- フィルタリング、並び替え、検索  
- モデル検証  

### 🧱 バックエンド
- ASP.NET Core 9 MVC + REST エンドポイント  
- Entity Framework Core 9  
- SQL Server データベース（LocalDB またはコンテナ）  
- Repository & Service パターン  
- 完全な async/await  
- 明確な責務分離  

### 🧩 フロントエンド
- ASP.NET MVC Razor  
- 支出の入力・編集・閲覧用のクリーンな UI  
- クライアント側バリデーション  
- API との REST 通信  

