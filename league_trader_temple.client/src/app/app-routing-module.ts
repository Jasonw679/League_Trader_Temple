import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { CardProduct } from './card-product/card-product';
import { Search } from './search/search';
import { ShoppingCart } from './shopping-cart/shopping-cart';
import { Login } from './login/login';
import { CreateAnAccount } from './create-an-account/create-an-account';
import { NotFound } from './not-found/not-found';

const routes: Routes = [
  { path: '', component: Home, pathMatch: 'full' },
  { path: 'card/:id', component: CardProduct },
  { path: 'search', component: Search },
  { path: 'shoppingcart', component: ShoppingCart },
  { path: 'login', component: Login },
  { path: 'create-account', component: CreateAnAccount },
  { path: '**', component: NotFound }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
