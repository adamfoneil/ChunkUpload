Dropzone.autoDiscover = false;
let dropZone = null;

function upload() {
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
            chunksUploaded: function (file) {
                console.log("chunksUploaded: " + file);
                const commitForm = document.getElementById("frmCommit");
                commitForm.fileName.value = file.name;                
                commitForm.submit();
            }
        });

    // upload button event listener
    $('#upload').on('click', upload);
});        


async function CompleteUpload(file) {
    await fetch()
}