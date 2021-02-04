"use strict";

import { getUrl } from "/js/main.js";
import { onLoginEvent } from "/js/google.js";

onInitIndex();

function onInitIndex() {
    onLoginEvent.push(onLoginIndex);
    console.info("Loaded index script!");
}

function onLoginIndex(user) {
    if (user.isSignedIn()) {
        window.location.replace(getUrl("/student-test", {}));
    }
}