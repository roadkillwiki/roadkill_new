import axios from "axios";
import Configuration from "../state/Configuration";

class PageService
{
    FINDHOMEPAGE_PATH = "/Pages/FindHomePage";
    PAGEVERSIONS_PATH = "/PageVersions/GetLatestVersion";

    getHomePage()
    {
        let baseUrl = Configuration.configuration.baseUrl;
        let fullUrl = `${baseUrl}${this.FINDHOMEPAGE_PATH}`;

        return axios
            .get(fullUrl)
            .then(response => { 
               return response.data;
            })
            .catch(function (error) {
                console.log(error);
                return error;
                //message.error(`Failed to load from ${fullUrl}`);
            });
    }

    getMarkdown(pageId)
    {
        let baseUrl = Configuration.configuration.baseUrl;
        let fullUrl = `${baseUrl}${this.PAGEVERSIONS_PATH}?pageId=${pageId}`;

        return axios
            .get(fullUrl)
            .then(response => { 
               return response.data.text;
            })
            .catch(function (error) {
                console.log(error);
                return error;
                //message.error(`Failed to load from ${fullUrl}`);
            });
    }
}

export default PageService;