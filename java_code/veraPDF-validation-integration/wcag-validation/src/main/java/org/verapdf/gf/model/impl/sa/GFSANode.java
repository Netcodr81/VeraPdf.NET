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

import org.verapdf.cos.COSKey;
import org.verapdf.wcag.algorithms.entities.INode;
import org.verapdf.wcag.algorithms.entities.SemanticNode;
import org.verapdf.wcag.algorithms.entities.maps.SemanticTypeMapper;

import java.util.List;

/**
 * Connects one adapted tagged structure element to the WCAG algorithms semantic tree.
 *
 * <p>The node carries the mapped semantic type, accumulated diagnostic codes and arguments,
 * COS object number, and normalized table attributes. Its children are loaded from the associated
 * {@link GFSAStructElem} only when the algorithm traverses the tree.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSANode}; the base
 * {@code org.verapdf.wcag.algorithms.entities.SemanticNode} has no .NET port yet. Bridge it to
 * {@code VeraPdf.Model.SALayer.ISAStructElem} and preserve lazy child loading and the nullable COS
 * object number.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSANode extends SemanticNode {

	private final GFSAStructElem structElem;

	/** Initializes algorithm semantics and attaches the structure element's table attributes. */
	public GFSANode(GFSAStructElem structElem) {
		super(SemanticTypeMapper.getSemanticType(structElem.getstandardType()), structElem.getErrorCodes(),
				structElem.getErrorArguments(), getObjectKeyNumber(structElem));
		this.structElem = structElem;
		setAttributesDictionary(new GFSAAttributesDictionary(structElem.getStructElemDictionary()));
	}
	
	private static Integer getObjectKeyNumber(GFSAStructElem structElem) {
		COSKey key = structElem.getStructElemDictionary().getObject().getKey();
		return key != null ? key.getNumber() : null;
	}

	/** Returns the model adapter that supplied this semantic node. */
	public GFSAStructElem getStructElem() {
        return structElem;
    }

	@Override
	/** Ensures model children have been parsed before returning algorithm children. */
	public List<INode> getChildren() {
		if (structElem.children == null) {
			structElem.parseChildren();
		}
		return super.getChildren();
	}

}
