import axios from "axios";
import Configuration from "../state/Configuration";

class MarkdownService {
    CONVERT_PATH = "/Markdown/ConvertToHtml";

    convertToHtml(markdown) {
        let baseUrl = Configuration.configuration.baseUrl;
        let fullUrl = `${baseUrl}${this.CONVERT_PATH}`;
        var config = {
            headers: {
                "Content-Type": "application/json",
                "accept": "text/plain"
            }
        };
        let rawBody = JSON.stringify(markdown);
        console.log(rawBody);

        return axios
            .post(fullUrl, rawBody, config)
            .then(response => {
                console.log(response.data);
                return response.data;
            })
            .catch(function (error) {
                console.log(error);
                return "";
                //message.error(`Failed to load from ${fullUrl}`);
            });
    }
}

export default MarkdownService;