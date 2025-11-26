# **Resolution Action System**

The **Resolution Action System** is an ASP.NET Core MVC application built for the Chillisoft technical assessment.
It provides a structured way to capture meetings, carry forward previous meeting items, update item statuses, and maintain a complete log of ongoing actions.

The system fully implements:

* **Use Case 1** – Capture New Meeting
* **Use Case 5** – Update Meeting Item Status

and includes clean UI design, proper exception handling, and working seeded data so the reviewer can test immediately.

---

## 📸 **Screenshots (Placeholders)**

### **Home Screen**


![Home Screen](https://raw.githubusercontent.com/liam-007/Resolution-Action-System/main/Screenshots/Screenshot%202025-11-26%20145224.png)



### **Create Meeting Page**


![Create Meeting](https://raw.githubusercontent.com/liam-007/Resolution-Action-System/main/Screenshots/Screenshot%202025-11-26%20145249.png)


### **Meeting Details Page**

```
![Meeting Details](images/details_placeholder.png)
```

---

# ⭐ **Features**

## **1. Capture New Meetings (Use Case 1)**

* Select meeting type: **MANCO**, **Finance**, **PTL**
* System auto-generates a unique meeting code (M01, F02, P03…)
* Enter meeting date and time
* *Optional:* Load previous items from any earlier meetings of the same type
* Choose which items to carry forward
* Create a new meeting and view it immediately

---

## **2. Update Existing Meetings**

* View a complete list of all meetings
* Filter meetings by type
* Open and inspect any meeting

---

## **3. Update Item Status Globally (Use Case 5)**

* Change the status of an item (Open / In Progress / Closed)
* Add meaningful comments
* Status updates apply **to all meetings containing that item**, ensuring consistency

---

## **4. Add New Items to a Meeting**

* Add items with Title, Responsible Person, and Description
* Items automatically get a new “Open” status entry

---

## **5. Clean & User-Friendly Interface**

* Bootstrap-based layout
* Clear navigation and professional styling
* Validation, error messages, and safe behaviour

---

# 🗄️ **Database Setup (SQL Server)**

The repository includes a SQL script that:

* Creates the required database tables
* Adds PK/FK constraints
* Inserts **Meeting Types**
* Seeds the database with sample MANCO and Finance meetings
* Adds sample items and statuses

This allows the system to run immediately without needing manual data entry.

---

## **1. Create the Database**

```sql
CREATE DATABASE MeetingMinutes;
GO
USE MeetingMinutes;
```

---

## **2. Run the SQL Setup Script**

The script will:

✔ Create all tables
✔ Add all constraints
✔ Insert meeting types
✔ Seed two example meetings
✔ Seed example items and statuses

This ensures the reviewer sees functionality instantly.

---

## **3. Update the Connection String**

Inside `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MeetingMinutes;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name.

---

# 🚀 **Running the Application**

### **Using Visual Studio**

1. Open the solution
2. Restore NuGet packages
3. Press **F5** to run

### **Using .NET CLI**

```
dotnet build
dotnet run
```

Navigate to the URL printed in the console.

---

# 🧭 **How to Use the System**

### **Home Page**

Choose one of:

* **Capture New Meeting**
* **Update Existing Meetings**
* **Exit**

### **Creating a New Meeting**

1. Select meeting type
2. Code auto-generates
3. Enter date/time
4. Load previous items (optional)
5. Select items
6. Create meeting

### **Updating a Status**

1. Open a meeting
2. Choose an item
3. Update status and comment
4. Save

---

# 🧩 **Tech Stack**

* **ASP.NET Core MVC**
* **Entity Framework Core**
* **SQL Server**
* **C# / .NET 6+**
* **Bootstrap 5**
* **SSMS** for DB management

---

# 🔍 **Notes for Reviewers**

* The SQL script includes seeded data so the system is fully testable immediately
* Code follows Chillisoft’s Use Case requirements
* Controller methods include explanatory comments
* Error messages and exception handling ensure safe usage
