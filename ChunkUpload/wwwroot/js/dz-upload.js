Dropzone.autoDiscover = false;
let dropZone = null;

function upload() {
    dropZone.options.autoProcessQueue = true;
    dropZone.processQueue();
}

$(document).ready(function () {
    const element = document.getElementById("dropzone");
    dropZone = new Dropzone(element,
    {
        chunking: true,
        forceChunking: true,
        chunkSize: 1024 * 1024 * 3,
        timeout: 3600000,
        addRemoveLinks: true, // this adds link to remove file from dropzone
        maxFilesize: 10485760, // 10 gig max file size
        autoProcessQueue: false, // this stops the chunks from immediate upload        
        chunksUploaded: async function (file, done) {
            const commitForm = document.getElementById("frmCommit");
            commitForm.fileName.value = file.name;
            let data = new FormData(commitForm);
            try {
                let response = await fetch("/Files/Commit", {
                    method: "post",
                    body: data,
                    headers: { "RequestVerificationToken": getCookie("RequestVerificationToken") }
                });
                console.log(response);
                done();
            } catch (e) {
                console.log(e);
            }
        },
        queueComplete: function () {
            dropZone.options.autoProcessQueue = false;
        }
    });

    // upload button event listener
    $('#upload').on('click', upload);
});

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length !== 2) {
        return null;
    }

    return parts.pop().split(";").shift();
}