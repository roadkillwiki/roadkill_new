//using Xunit;
//using Roadkill.Core.Text.Menu;
//using Roadkill.Core.Text.Plugins;
//using Roadkill.Core.Text.TextMiddleware;

//namespace Roadkill.Tests.Unit.Text.TextMiddleware
//{
//    public class TextPluginBeforeParseMiddlewareTests
//    {
//        private MocksAndStubsContainer _container;
//        private TextPluginRunner _pluginRunner;
//        private PluginFactoryMock _pluginFactory;

//        public TextPluginBeforeParseMiddlewareTests()
//        {
//            _container = new MocksAndStubsContainer();

//            _pluginFactory = _container.PluginFactory;
//            _pluginRunner = new TextPluginRunner(_pluginFactory);
//        }

//        [Fact]
//        public void should_fire_beforeparse_in_textplugin()
//        {
//            // Arrange
//            string markupFragment = "This is my ~~~usertoken~~~";
//            string expectedHtml = "This is my <span>usertoken</span>";

//            TextPluginStub plugin = new TextPluginStub();
//            plugin.SettingsRepository = new SettingsRepositoryMock();
//            plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
//            plugin.Settings.IsEnabled = true;
//            _pluginFactory.RegisterTextPlugin(plugin);

//            var middleware = new TextPluginBeforeParseMiddleware(_pluginRunner);
//            var pageHtml = new PageHtml();
//            pageHtml.Html = markupFragment;

//            // Act
//            PageHtml actualPageHtml = middleware.Invoke(pageHtml);

//            // Assert
//            Assert.Equal(expectedHtml, actualPageHtml.Html);
//        }
//    }
//}