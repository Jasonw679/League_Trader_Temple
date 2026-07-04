import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { Header } from './header/header';
import { Footer } from './footer/footer';
import { Card } from './card/card';
import { Home } from './home/home';
import { Search } from './search/search';
import { NotFound } from './not-found/not-found';
import { CardProduct } from './card-product/card-product';
import { ShoppingCart } from './shopping-cart/shopping-cart';
import { Login } from './login/login';
import { CreateAnAccount } from './create-an-account/create-an-account';

@NgModule({
  declarations: [
    App,
    Header,
    Footer,
    Home,
    Search,
    Card,
    NotFound,
    CardProduct,
    ShoppingCart,
    Login,
    CreateAnAccount,
  ],
  imports: [BrowserModule, HttpClientModule, AppRoutingModule, FormsModule],
  providers: [provideBrowserGlobalErrorListeners()],
  bootstrap: [App],
})
export class AppModule {}
