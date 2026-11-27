/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.sa;

import org.verapdf.model.GenericModelObject;
import org.verapdf.model.salayer.SAObject;
import org.verapdf.wcag.algorithms.entities.IObject;

import java.util.*;
import java.util.stream.Collectors;

/**
 * Shared SA model adapter for WCAG algorithm objects and their diagnostics.
 *
 * <p>Algorithm entities may accumulate semantic error codes and associated arguments while
 * analysis runs. This class converts those values to the string lists expected by model/profile
 * consumers and exposes the entity's geometric context and recognized structure identifier.</p>
 *
 * <p><b>.NET porting notes:</b> Port as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAObject} implementing
 * {@code VeraPdf.Model.SALayer.ISAObject}. Use {@code List<T>} internally and expose
 * {@code IReadOnlyList<T>} where the model contract allows; Java {@code Object} arguments need
 * deliberate string conversion, and the algorithm {@code IObject} is not yet ported.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAObject extends GenericModelObject implements SAObject {

	private final List<Integer> errorCodes = new LinkedList<>();
	private final List<List<Object>> errorArguments = new LinkedList<>();
	protected IObject object;

	/** Creates an SA object shell before its algorithm entity is attached. */
	public GFSAObject(String type) {
		super(type);
	}

	/** Wraps an existing algorithm entity under the supplied profile-visible type name. */
	public GFSAObject(IObject object, String type) {
		super(type);
		this.object = object;
	}

	@Override
	/** Converts diagnostic numeric codes to their string model representation. */
	public List<String> geterrorCodes() {
		return errorCodes.stream().map(Objects::toString).collect(Collectors.toList());
	}

	/** Returns the original numeric diagnostic codes for algorithm-node construction. */
	public List<Integer> getErrorCodes() {
		return errorCodes;
	}

	/** Returns the raw diagnostic arguments before model string conversion. */
	public List<List<Object>> getErrorArguments() {
		return errorArguments;
	}

	@Override
	/** Converts each diagnostic argument to the profile-facing string form. */
	public List<List<String>> geterrorArguments() {
		List<List<String>> list = new LinkedList<>();
		for (List<Object> arguments : errorArguments) {
			list.add(arguments.stream().map(Object::toString).collect(Collectors.toList()));
		}
		return list;
	}

	@Override
	/** Returns the algorithm bounding box's location as human-readable context. */
	public String getContext() {
		return object.getBoundingBox().getLocation();
	}

	@Override
	/** Returns the recognized structure identifier with the model's {@code id:} prefix. */
	public String getstructureID() {
		if (object.getRecognizedStructureId() != null) {
			return "id:" + object.getRecognizedStructureId();
		}
		return null;
	}

	protected void setObject(IObject object) {
		this.object = object;
	}

	protected IObject getObject() {
		return object;
	}
}
