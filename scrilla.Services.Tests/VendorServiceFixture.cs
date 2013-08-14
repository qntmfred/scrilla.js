﻿using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace scrilla.Services.Tests
{
	public class VendorServiceFixture : BaseFixture<VendorService>
	{
		public VendorServiceFixture()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
			_sut = _fixture.Create<VendorService>();
		}

		[Fact]
		public void GetVendor_ExistingVendor_WithNullDefaultCategoryId()
		{
			var vendorName = "test vendor";
			int? defaultCategoryId = null;

			// create test vendor
			var addVendorResult =  _sut.AddVendor(vendorName, defaultCategoryId);
			Assert.False(addVendorResult.HasErrors);
			Assert.Equal(vendorName, addVendorResult.Result.Name);
			Assert.Equal(defaultCategoryId, addVendorResult.Result.DefaultCategoryId);

			// act
			var result =  _sut.GetVendor(addVendorResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(vendorName, result.Result.Name);
			Assert.Equal(defaultCategoryId, result.Result.DefaultCategoryId);

			// cleanup
			 _sut.DeleteVendor(addVendorResult.Result.Id);
		}

		[Fact]
		public void GetVendor_ExistingVendor_WithDefaultCategoryId()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var vendorName = "test vendor";
			var categoryName = "test category";

			// create test category
			var addCategoryResult = categoryService.AddCategory(categoryName);
			Assert.False(addCategoryResult.HasErrors);
			Assert.Equal(categoryName, addCategoryResult.Result.Name);

			// create test vendor
			var addVendorResult =  _sut.AddVendor(vendorName, addCategoryResult.Result.Id);
			Assert.False(addVendorResult.HasErrors);
			Assert.Equal(vendorName, addVendorResult.Result.Name);
			Assert.Equal(addCategoryResult.Result.Id, addVendorResult.Result.DefaultCategoryId);

			// act
			var result =  _sut.GetVendor(addVendorResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(vendorName, result.Result.Name);
			Assert.Equal(addCategoryResult.Result.Id, result.Result.DefaultCategoryId);

			// cleanup
			 _sut.DeleteVendor(addVendorResult.Result.Id);
			categoryService.DeleteCategory(addCategoryResult.Result.Id);
		}

		[Fact]
		public void GetVendor_NonExistantVendor()
		{
			var nonExistantVendorId = -1;

			// act
			var result =  _sut.GetVendor(nonExistantVendorId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}

		[Fact]
		public void GetVendorMap_ExistingVendorMap()
		{
			var vendorName = "test vendor";
			var description = "test vendor map";

			// add a test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// add a test vendor map
			var addVendorMapResult = _sut.AddVendorMap(addVendorResult.Result.Id, description);
			Assert.False(addVendorMapResult.HasErrors);

			// act
			var result = _sut.GetVendorMap(addVendorResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(description, result.Result.Description);
			Assert.Equal(addVendorResult.Result.Id, result.Result.VendorId);

			// cleanup
			_sut.DeleteVendorMap(addVendorMapResult.Result.Id);
			_sut.DeleteVendor(addVendorResult.Result.Id);
		}

		[Fact]
		public void GetVendorMap_NonExistantVendorMap()
		{
			var nonExistantVendorMapId = -1;

			// act
			var result = _sut.GetVendorMap(nonExistantVendorMapId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}

		[Fact]
		public void GetAllVendors_OneVendorExistsAfterAddingAVendor()
		{
			var vendorResult = _sut.GetAllVendors();
			Assert.False(vendorResult.HasErrors);
			Assert.Empty(vendorResult.Result);

			// create test account
			var name = "test vendor";
			var addVendorResult = _sut.AddVendor(name);
			Assert.False(addVendorResult.HasErrors);

			// act
			vendorResult = _sut.GetAllVendors();
			Assert.False(vendorResult.HasErrors);
			Assert.Equal(1, vendorResult.Result.Count());

			// cleanup
			_sut.DeleteVendor(addVendorResult.Result.Id);
		}

		[Fact]
		public void AddVendor_NullDefaultCategoryId()
		{
			var name = "test vendor";
			int? defaultCategoryId = null;

			// act
			var result = _sut.AddVendor(name, defaultCategoryId);
			Assert.False(result.HasErrors);
			Assert.Equal(name, result.Result.Name);
			Assert.Equal(defaultCategoryId, result.Result.DefaultCategoryId);

			// cleanup
			_sut.DeleteVendor(result.Result.Id);
		}

		[Fact]
		public void AddVendor_NonNullDefaultCategoryId()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var name = "test vendor";

			// get a default category
			var categoryName = "test category";
			var categoryResult = categoryService.AddCategory(categoryName);
			Assert.False(categoryResult.HasErrors);

			// act
			var result = _sut.AddVendor(name, categoryResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(name, result.Result.Name);
			Assert.Equal(categoryResult.Result.Id, result.Result.DefaultCategoryId);

			// cleanup
			_sut.DeleteVendor(result.Result.Id);
			categoryService.DeleteCategory(categoryResult.Result.Id);
		}

		[Fact]
		public void AddVendorMap_ExistingVendor()
		{
			var vendorName = "test vendor";
			var description = "test vendor map";

			// add a test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// act
			var result = _sut.AddVendorMap(addVendorResult.Result.Id, description);
			Assert.False(result.HasErrors);
			Assert.Equal(description, result.Result.Description);
			Assert.Equal(addVendorResult.Result.Id, result.Result.VendorId);

			// cleanup
			_sut.DeleteVendorMap(result.Result.Id);
			_sut.DeleteVendor(addVendorResult.Result.Id);
		}

		[Fact]
		public void AddVendorMap_NonExistantVendor()
		{
			var description = "test vendor map";
			var nonExistantVendorId = -1;

			// act
			var result = _sut.AddVendorMap(nonExistantVendorId, description);
			Assert.True(result.HasErrors);
		}

		[Fact]
		public void DeleteVendor_ExistingVendor()
		{
			var name = "test vendor";

			// add a test vendor
			var addResult = _sut.AddVendor(name);
			Assert.False(addResult.HasErrors);

			// delete the test vendor
			var deletionResult = _sut.DeleteVendor(addResult.Result.Id);
			Assert.False(deletionResult.HasErrors);

			// make sure the test vendor does not exist
			var getResult = _sut.GetVendor(addResult.Result.Id);
			Assert.True(getResult.HasErrors);
			Assert.True(getResult.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}

		[Fact]
		public void DeleteVendor_NonExistantVendor()
		{
			var nonExistantVendorId = -1;

			// act
			var result = _sut.DeleteVendor(nonExistantVendorId);
			Assert.True(result.HasErrors);
		}

		[Fact]
		public void DeleteVendorMap_ExistingVendorMap()
		{
			var vendorName = "test vendor";
			var description = "test vendor map";

			// add a test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// add a test vendor map
			var addVendorMapResult = _sut.AddVendorMap(addVendorResult.Result.Id, description);
			Assert.False(addVendorMapResult.HasErrors);

			// delete the test vendor map
			var deletionResult = _sut.DeleteVendorMap(addVendorMapResult.Result.Id);
			Assert.False(deletionResult.HasErrors);

			// make sure the test vendor map does not exist
			var getResult = _sut.GetVendorMap(addVendorMapResult.Result.Id);
			Assert.True(getResult.HasErrors);
			Assert.True(getResult.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));

			// cleanup
			_sut.DeleteVendorMap(addVendorMapResult.Result.Id);
			_sut.DeleteVendor(addVendorResult.Result.Id);
		}

		[Fact]
		public void DeleteVendorMap_NonExistantVendorMap()
		{
			var nonExistantVendorMapId = -1;

			// act
			var result = _sut.DeleteVendorMap(nonExistantVendorMapId);
			Assert.True(result.HasErrors);
		}

		[Fact]
		public void UpdateVendor_ExistingVendor_And_ExistingCategory()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var vendorName = "test vendor";
			var newVendorName = "new test vendor";
			var categoryName = "test category";

			// add test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// add test category
			var addCategoryResult = categoryService.AddCategory(categoryName);
			Assert.False(addCategoryResult.HasErrors);

			// act
			var result = _sut.UpdateVendor(addVendorResult.Result.Id, newVendorName, addCategoryResult.Result.Id, true);
			Assert.False(result.HasErrors);
			Assert.Equal(newVendorName, result.Result.Name);
			Assert.Equal(addCategoryResult.Result.Id, result.Result.DefaultCategoryId);

			// cleanup
			_sut.DeleteVendor(result.Result.Id);
			categoryService.DeleteCategory(addCategoryResult.Result.Id);
		}

		[Fact]
		public void UpdateVendor_NonExistantVendor()
		{
			var nonExistantVendorId = -1;
			var newVendorName = "new vendor";

			// act
			var result = _sut.UpdateVendor(nonExistantVendorId, newVendorName, null, true);
			Assert.True(result.HasErrors);
		}

		[Fact]
		public void UpdateVendorName_ExistingVendor()
		{
			var vendorName = "test vendor";
			var newVendorName = "new test vendor";

			// add test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// act
			var result = _sut.UpdateVendorName(addVendorResult.Result.Id, newVendorName);
			Assert.False(result.HasErrors);

			// cleanup
			_sut.DeleteVendor(result.Result.Id);
		}

		[Fact]
		public void UpdateVendorName_NonExistantVendor()
		{
			var nonExistantVendorId = -1;
			var newVendorName = "new vendor";

			// act
			var result = _sut.UpdateVendorName(nonExistantVendorId, newVendorName);
			Assert.False(result.HasErrors);
		}

		[Fact]
		public void UpdateVendorDefaultCategory()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var vendorName = "test vendor";
			var categoryName = "test category";

			// add test vendor
			var addVendorResult = _sut.AddVendor(vendorName);
			Assert.False(addVendorResult.HasErrors);

			// add test category
			var addCategoryResult = categoryService.AddCategory(categoryName);
			Assert.False(addCategoryResult.HasErrors);

			// act
			var result = _sut.UpdateVendorDefaultCategory(addVendorResult.Result.Id, null);
			Assert.False(result.HasErrors);
			Assert.Equal(null, result.Result.DefaultCategoryId);

			// cleanup
			_sut.DeleteVendor(result.Result.Id);
			categoryService.DeleteCategory(addCategoryResult.Result.Id);
		}
	}
}
