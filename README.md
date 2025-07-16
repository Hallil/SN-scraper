# CSA Scraper

ServiceNow ExamTopics Scraper
Author: Halil
GitHub Repository: https://github.com/Hallil/SN-scraper.git

This tool automatically extracts ServiceNow HR (CIS-HR) exam questions from ExamTopics.com and saves them in a Microsoft Word document (.docx). The exported file includes questions, answer choices, and a link to the original discussion page so you can manually check for the correct answer.

🔧 Requirements
A Windows laptop or computer

Visual Studio 2022 or later (Community Edition) – free to download and use

🧱 Setup Instructions
1. Install Visual Studio
Visit: https://visualstudio.microsoft.com/

Click Free Download under Visual Studio Community

During the installation:

Make sure to select “.NET desktop development”

Then click Install

2. Clone the GitHub Repository
Open Visual Studio

Click Clone a repository

Paste this link under Repository Location:

https://github.com/Hallil/SN-scraper.git
Choose a folder to save the project

Click Clone

3. Change Output File Path (Optional but recommended)
In Visual Studio, open the file: Program.cs

Go to line 33

Find this line:

csharp
string outputPath = @"C:\Users\halil\Desktop\HR vragen.docx";
Change the path to any folder on your computer, for example:

csharp
string outputPath = @"C:\Users\yourname\Documents\SN_Exam_Questions.docx";
This is where the Word file with all the questions will be saved.

4. (Optional) Change the Exam Topic
By default, this tool scrapes CIS-HR discussions.

If you want a different ServiceNow exam topic (like CIS-ITSM, CIS-SM, etc.):

Open Program.cs

Go to line 95
csharp
Change the phrase "Exam CIS-HR topic" to the topic label used on the website links, e.g.:

// Change:
contains(text(), 'Exam CIS-HR topic')

// To:
contains(text(), 'Exam CIS-ITSM topic')
Make sure to match the title used on https://www.examtopics.com/discussions/servicenow/.

▶️ Run the Application
In Visual Studio:

Make sure the project selected at the top is: CSA Scraper

Click Build > Build Solution

Press the green play button (CSA Scraper) to start

You will see a black console window appear.

⌛ Wait for the process to complete
The program will go through all posts and questions one by one.

This may take several minutes.

Once finished, it will show:

Scraping completed! Total questions found: [number]
Press the spacebar in the console window to close it.

📁 View Your Output
Your questions will be saved in the Word file at the folder path you set earlier.

Each question includes:
The question text
Answer choices
A link to view the full discussion and suggested solutions online

✅ Add Correct Answers Manually
Open the .docx file from your folder
Under each question, you'll find a line like:
text
View this question online: https://www.examtopics.com/discussions/servicenow/view/12345/
Visit that link in your browser

Click the "Solution" tab to view community-provided answers

In the Word file:
Replace that line with:

Suggested answer: [write the correct answer here]
After you're done, Save the file as a .txt file for easy sharing or reference:

File > Save As > choose .txt as the file type

📘 Example Output
Question: What is the primary purpose of the HR Service Portal?
A. Allow employees to submit tickets  
B. Automate knowledge base creation  
C. Provide support agents with analytics  
D. Integrate with third-party systems  
Suggested answer: A

Question: HR Profiles may be created for multiple employees using conditions and criteria in which module?
A. Create Human Resources Profile
B. Create new Case
C. Generate HR Profiles
D. Bulk Cases
Suggested answer: C

❓ FAQ
⚠ The scraper crashes or stops?
This is usually caused by the website blocking too many requests.

Wait a bit and try again.
You can optionally increase the delay between requests (Task.Delay(...)) in the code.

🔒 Is this legal?
Use this tool strictly for personal and educational purposes.
Always respect ExamTopics’ terms of use and community policies.
