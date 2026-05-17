import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
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

@NgModule({
  declarations: [App, Header, Footer, Home, Search, Card, NotFound, CardProduct, ShoppingCart],
  imports: [BrowserModule, HttpClientModule, AppRoutingModule],
  providers: [provideBrowserGlobalErrorListeners()],
  bootstrap: [App],
})
export class AppModule {}
