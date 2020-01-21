import React, {Component} from "react";
import MarkdownService from "../services/MarkdownService";

class PageComponent extends Component
{
    constructor(props) {
        super(props)
        this.state = {
            html: ""
        };
    }

    componentDidMount()
    {
        var service = new MarkdownService();
        service
            .convertToHtml("**hello**")
            .then(html =>
            {
                this.setState({ html: html });
            });
    }

    render()
    {
        if (this.state.html)
        {
            let unsafeHtml = {
                __html : this.state.html
            };

            return (
                <div dangerouslySetInnerHTML={ unsafeHtml } />
            );
        }

        return null;
    }
}

export default PageComponent;