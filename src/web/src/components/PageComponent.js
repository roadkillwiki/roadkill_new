import React, {Component} from "react";
import PageService from "../services/PageService";
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
        var pageService = new PageService();
        var markdownService = new MarkdownService();

        pageService
            .getHomePage()
            .then(page => pageService.getMarkdown(page.id))
            .then(text => markdownService.convertToHtml(text))
            .then(html => this.setState({ html: html }));
    }

    render()
    {
        if (this.state.html)
        {
            let unsafeHtml = {
                __html : this.state.html
            };

            return (
                <div style={{border: "1px solid #ccc"}} dangerouslySetInnerHTML={ unsafeHtml } />
            );
        }

        return null;
    }
}

export default PageComponent;