function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length !== 2) {
        return null;
    }

    return parts.pop().split(";").shift();
}

async function stream(form) {
    const formData = new FormData(form);

    try {
        const response = await fetch(form.action,
        {
            method: form.method,
            body: formData,
            headers: {
                'RequestVerificationToken': getCookie('RequestVerificationToken')
            }
        });

        console.log(response);
    } catch (error) {
        console.error(error);
    }
}
