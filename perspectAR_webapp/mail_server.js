const express = require('express');
const nodemailer = require('nodemailer');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const port = 3000;

app.use(cors()); // Enable CORS for all routes
app.use(bodyParser.json());

// Configure nodemailer for Outlook
const transporter = nodemailer.createTransport({
    host: 'smtp.office365.com',
    port: 587,
    secure: false,
    auth: {
        user: 'demo@demo.com', // replace with your Outlook email
        pass: 'demo@demo.com' // replace with your email password
    }
});

app.post('/send-email', (req, res) => {
    const { csvContent, filename, email } = req.body;

    const mailOptions = {
        from: 'demo@demo.com', // replace with your Outlook email
        to: email,
        subject: 'CSV Log File',
        text: 'Please find the attached CSV log file.',
        attachments: [
            {
                filename: filename,
                content: csvContent
            }
        ]
    };

    transporter.sendMail(mailOptions, (error, info) => {
        if (error) {
            return res.status(500).json({ success: false, error: error.message });
        }
        res.json({ success: true, message: 'Email sent: ' + info.response });
    });
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
});
