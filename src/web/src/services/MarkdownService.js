import axios from "axios";
import Configuration from "../state/Configuration";

class MarkdownService
{
    CONVERT_PATH = "/Markdown/ConvertToHtml";

    convertToHtml(markdown)
    {
        let baseUrl = Configuration.configuration.baseUrl;
        let fullUrl = `${baseUrl}${this.CONVERT_PATH}`;
        var config = {
            headers: {
                "Content-Type": "application/json",
                "accept" : "text/plain"
            }
        };
        let rawBody = `"${markdown}"`; // for ASP.NET Core's benefit it should be in quotes

        return axios
            .post(fullUrl, rawBody, config)
            .then(response => { 
               return response.data;
            })
            .catch(function (error) {
                console.log(error);
                return error;
                //message.error(`Failed to load from ${fullUrl}`);
            });
    }
}

export default MarkdownService;