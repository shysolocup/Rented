const { getDefaultAutoSelectFamily } = require("net");

function sleep(time) {
    return new Promise(resolve => setTimeout(resolve, time*1000));
}

let commitSizes = [];

fetch("https://api.github.com/repos/shysolocup/Rented/commits").then( res => {
    console.log(res.statusText);
    console.log(res.headers.get("x-ratelimit-remaining"));

    if (!res.ok) return;

    res.json().then( async commits => {
        console.log(commits);

        for (let i = 0; i < commits.length; i++) {
            await sleep(1);

            let sha = commits[i];

            fetch(sha.commit.url).then( res2 => {
                console.log(res2.statusText);
                console.log(res.headers.get("x-ratelimit-remaining"));
                
                if (res2.ok) {
                    res2.json().then( data => {
                        if (i == 0) console.log(data);
                    })
                }
            })
        }
    });
});

setInterval(() => {}, 1 << 30);